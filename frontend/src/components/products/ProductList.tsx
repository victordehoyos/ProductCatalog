'use client';

import { useState, useEffect } from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  Chip,
  IconButton,
  Alert,
  CircularProgress,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  TextField,
  MenuItem,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControlLabel,
  Checkbox,
  Paper,
  InputAdornment,
} from '@mui/material';
import { Edit, Delete, Inventory, Search } from '@mui/icons-material';
import { Product, UpdateProductDto } from '@/types';
import { apiService } from '@/services/api';
import { getErrorMessage } from '@/lib/error-handler';

interface StockAdjustmentForm {
  quantity: number;
  isDecrement: boolean;
}

export default function ProductList() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Estados para paginación y filtros
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState('name');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');
  
  // Estados para modales
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [stockModalOpen, setStockModalOpen] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  
  // Estados para formularios
  const [editForm, setEditForm] = useState<UpdateProductDto>({
    name: '',
    description: '',
    price: 0,
  });
  
  const [stockForm, setStockForm] = useState<StockAdjustmentForm>({
    quantity: 0,
    isDecrement: false,
  });

  // Estado para controlar la hidratación
  const [isClient, setIsClient] = useState(false);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      const data = await apiService.getProducts();
      setProducts(data);
      setError(null);
    } catch (err) {
      setError('Failed to fetch products');
      console.error('Error fetching products:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    setIsClient(true);
    fetchProducts();
  }, []);

  // Filtrar y ordenar productos
  const filteredAndSortedProducts = products
    .filter(product =>
      product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      product.description?.toLowerCase().includes(searchTerm.toLowerCase())
    )
    .sort((a, b) => {
      const aValue = a[sortBy as keyof Product];
      const bValue = b[sortBy as keyof Product];
      
      if (typeof aValue === 'string' && typeof bValue === 'string') {
        return sortOrder === 'asc' 
          ? aValue.localeCompare(bValue)
          : bValue.localeCompare(aValue);
      }
      
      if (typeof aValue === 'number' && typeof bValue === 'number') {
        return sortOrder === 'asc' ? aValue - bValue : bValue - aValue;
      }
      
      return 0;
    });

  // Productos paginados
  const paginatedProducts = filteredAndSortedProducts.slice(
    page * rowsPerPage,
    page * rowsPerPage + rowsPerPage
  );

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to delete this product?')) return;

    try {
      await apiService.deleteProduct(id);
      await fetchProducts();
    } catch (err) {
      const errorMessage = getErrorMessage(err);
      setError(errorMessage);
      console.error('Error deleting product:', err);
    }
  };

  const handleEditClick = (product: Product) => {
    setSelectedProduct(product);
    setEditForm({
      name: product.name,
      description: product.description || '',
      price: product.price,
    });
    setEditModalOpen(true);
  };

  const handleStockClick = (product: Product) => {
    setSelectedProduct(product);
    setStockForm({
      quantity: 0,
      isDecrement: false,
    });
    setStockModalOpen(true);
  };

  const handleEditSubmit = async () => {
    if (!selectedProduct) return;

    try {
      await apiService.updateProduct(selectedProduct.id, editForm);
      await fetchProducts();
      setEditModalOpen(false);
      setSelectedProduct(null);
    } catch (err) {
      const errorMessage = getErrorMessage(err);
      setError(errorMessage);
      console.error('Error updating product:', err);
    }
  };

  const handleStockSubmit = async () => {
    if (!selectedProduct) return;

    try {
      if (stockForm.isDecrement) {
        await apiService.decreaseStock(selectedProduct.id, stockForm.quantity);
      } else {
        await apiService.increaseStock(selectedProduct.id, stockForm.quantity);
      }
      await fetchProducts();
      setStockModalOpen(false);
      setSelectedProduct(null);
    } catch (err) {
      const errorMessage = getErrorMessage(err);
      setError(errorMessage);
      console.error('Error adjusting stock:', err);
    }
  };

  const handleSort = (column: string) => {
    if (sortBy === column) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(column);
      setSortOrder('asc');
    }
  };

  const handleChangePage = (event: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  // Evitar renderizado hasta que esté en el cliente
  if (!isClient) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight={200}>
        <CircularProgress />
      </Box>
    );
  }

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight={200}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box suppressHydrationWarning>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h5" component="h1">
          Products ({filteredAndSortedProducts.length})
        </Typography>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Filtros y búsqueda */}
      <Paper sx={{ p: 2, mb: 2 }}>
        <Box 
          sx={{ 
            display: 'flex', 
            flexDirection: { xs: 'column', md: 'row' },
            gap: 2 
          }}
        >
          <Box sx={{ width: { xs: '100%', md: 'calc(33.333% - 16px)' } }}>
            <TextField
              fullWidth
              placeholder="Search products..."
              value={searchTerm}
              onChange={(e) => {
                setSearchTerm(e.target.value);
                setPage(0);
              }}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <Search />
                  </InputAdornment>
                ),
              }}
            />
          </Box>
          <Box sx={{ width: { xs: '100%', md: 'calc(33.333% - 16px)' } }}>
            <TextField
              fullWidth
              select
              label="Sort by"
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value)}
            >
              <MenuItem value="name">Name</MenuItem>
              <MenuItem value="price">Price</MenuItem>
              <MenuItem value="stock">Stock</MenuItem>
            </TextField>
          </Box>
          <Box sx={{ width: { xs: '100%', md: 'calc(33.333% - 16px)' } }}>
            <TextField
              fullWidth
              select
              label="Order"
              value={sortOrder}
              onChange={(e) => setSortOrder(e.target.value as 'asc' | 'desc')}
            >
              <MenuItem value="asc">Ascending</MenuItem>
              <MenuItem value="desc">Descending</MenuItem>
            </TextField>
          </Box>
        </Box>
      </Paper>

      {filteredAndSortedProducts.length === 0 ? (
        <Card>
          <CardContent>
            <Box textAlign="center" py={4}>
              <Inventory sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
              <Typography variant="h6" color="text.secondary">
                {searchTerm ? 'No products match your search' : 'No products found'}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {searchTerm ? 'Try adjusting your search terms' : 'Create your first product to get started'}
              </Typography>
            </Box>
          </CardContent>
        </Card>
      ) : (
        <>
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell 
                    onClick={() => handleSort('name')}
                    sx={{ cursor: 'pointer', fontWeight: 'bold' }}
                  >
                    Name {sortBy === 'name' && (sortOrder === 'asc' ? '↑' : '↓')}
                  </TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell 
                    onClick={() => handleSort('price')}
                    sx={{ cursor: 'pointer', fontWeight: 'bold' }}
                  >
                    Price {sortBy === 'price' && (sortOrder === 'asc' ? '↑' : '↓')}
                  </TableCell>
                  <TableCell 
                    onClick={() => handleSort('stock')}
                    sx={{ cursor: 'pointer', fontWeight: 'bold' }}
                  >
                    Stock {sortBy === 'stock' && (sortOrder === 'asc' ? '↑' : '↓')}
                  </TableCell>
                  <TableCell align="center">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {paginatedProducts.map((product) => (
                  <TableRow key={product.id} hover>
                    <TableCell>
                      <Typography variant="subtitle2" fontWeight="bold">
                        {product.name}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" color="text.secondary">
                        {product.description || 'No description'}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" fontWeight="bold">
                        ${product.price.toFixed(2)}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={`${product.stock} in stock`}
                        color={product.stock > 10 ? 'success' : product.stock > 0 ? 'warning' : 'error'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="center">
                      <IconButton 
                        size="small" 
                        color="primary"
                        onClick={() => handleEditClick(product)}
                      >
                        <Edit />
                      </IconButton>
                      <IconButton 
                        size="small" 
                        color="secondary"
                        onClick={() => handleStockClick(product)}
                      >
                        <Inventory />
                      </IconButton>
                      <IconButton 
                        size="small" 
                        color="error"
                        onClick={() => handleDelete(product.id)}
                      >
                        <Delete />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          <TablePagination
            rowsPerPageOptions={[5, 10, 25]}
            component="div"
            count={filteredAndSortedProducts.length}
            rowsPerPage={rowsPerPage}
            page={page}
            onPageChange={handleChangePage}
            onRowsPerPageChange={handleChangeRowsPerPage}
          />
        </>
      )}

      {/* Modal para editar producto */}
      <Dialog open={editModalOpen} onClose={() => setEditModalOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Edit Product</DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            label="Name"
            value={editForm.name}
            onChange={(e) => setEditForm({ ...editForm, name: e.target.value })}
            margin="normal"
          />
          <TextField
            fullWidth
            label="Description"
            value={editForm.description}
            onChange={(e) => setEditForm({ ...editForm, description: e.target.value })}
            margin="normal"
            multiline
            rows={3}
          />
          <TextField
            fullWidth
            label="Price"
            type="number"
            value={editForm.price}
            onChange={(e) => setEditForm({ ...editForm, price: parseFloat(e.target.value) })}
            margin="normal"
            InputProps={{
              startAdornment: <InputAdornment position="start">$</InputAdornment>,
            }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditModalOpen(false)}>Cancel</Button>
          <Button onClick={handleEditSubmit} variant="contained" color="primary">
            Save Changes
          </Button>
        </DialogActions>
      </Dialog>

      {/* Modal para ajustar stock */}
      <Dialog open={stockModalOpen} onClose={() => setStockModalOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          Adjust Stock - {selectedProduct?.name}
        </DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            label="Quantity"
            type="number"
            value={stockForm.quantity}
            onChange={(e) => setStockForm({ ...stockForm, quantity: parseInt(e.target.value) || 0 })}
            margin="normal"
            InputProps={{
              inputProps: { min: 0 }
            }}
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={stockForm.isDecrement}
                onChange={(e) => setStockForm({ ...stockForm, isDecrement: e.target.checked })}
                color="primary"
              />
            }
            label="Decrease stock instead of increase"
          />
          {selectedProduct && (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              Current stock: {selectedProduct.stock} → New stock: {
                stockForm.isDecrement 
                  ? selectedProduct.stock - stockForm.quantity
                  : selectedProduct.stock + stockForm.quantity
              }
            </Typography>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setStockModalOpen(false)}>Cancel</Button>
          <Button 
            onClick={handleStockSubmit} 
            variant="contained" 
            color={stockForm.isDecrement ? 'warning' : 'primary'}
            disabled={stockForm.quantity <= 0}
          >
            {stockForm.isDecrement ? 'Decrease Stock' : 'Increase Stock'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}