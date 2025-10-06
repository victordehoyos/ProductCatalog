'use client';

import { useState, useEffect } from 'react';
import {
  Card,
  CardContent,
  TextField,
  Button,
  Box,
  Typography,
  Alert,
  MenuItem,
} from '@mui/material';
import { AddShoppingCart } from '@mui/icons-material';
import { Product, CreateOrderDto } from '@/types';
import { apiService } from '@/services/api';
import { getErrorMessage } from '@/lib/error-handler';

export default function CreateOrderForm() {
  const [products, setProducts] = useState<Product[]>([]);
  const [formData, setFormData] = useState<CreateOrderDto>({
    productId: 0,
    quantity: 1,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const data = await apiService.getProducts();
        setProducts(data);
      } catch (err) {
        console.error('Error fetching products:', err);
        setError(getErrorMessage(err));
      }
    };
    fetchProducts();
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'productId' || name === 'quantity' ? Number(value) : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setSuccess(false);

    try {
      await apiService.createOrder(formData);
      setFormData({ productId: 0, quantity: 1 });
      setSuccess(true);
      setTimeout(() => window.location.reload(), 1500);
    } catch (err) {
      const errorMessage = getErrorMessage(err);
      setError(errorMessage);
      console.error('Error creating order:', err);
    } finally {
      setLoading(false);
    }
  };

  const selectedProduct = products.find(p => p.id === formData.productId);
  const maxQuantity = selectedProduct?.stock || 0;
  const isFormValid = formData.productId > 0 && formData.quantity > 0 && formData.quantity <= maxQuantity;

  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Create New Order
        </Typography>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

        {success && (
          <Alert severity="success" sx={{ mb: 2 }}>
            Order created successfully!
          </Alert>
        )}

        <Box component="form" onSubmit={handleSubmit} sx={{ display: 'grid', gap: 2 }}>
          <TextField
            select
            label="Product"
            name="productId"
            value={formData.productId}
            onChange={handleChange}
            required
            fullWidth
            error={formData.productId === 0 && formData.quantity > 0}
          >
            <MenuItem value={0}>Select a product</MenuItem>
            {products.map((product) => (
              <MenuItem 
                key={product.id} 
                value={product.id}
                disabled={product.stock === 0}
              >
                {product.name} - ${product.price.toFixed(2)} 
                {product.stock === 0 ? ' (Out of stock)' : ` (Stock: ${product.stock})`}
              </MenuItem>
            ))}
          </TextField>

          <TextField
            label="Quantity"
            name="quantity"
            type="number"
            value={formData.quantity}
            onChange={handleChange}
            required
            inputProps={{ 
              min: 1, 
              max: maxQuantity 
            }}
            helperText={
              selectedProduct ? 
                `Available: ${maxQuantity} units` : 
                'Please select a product first'
            }
            error={formData.quantity > maxQuantity || formData.quantity < 1}
            disabled={!formData.productId}
            fullWidth
          />

          {selectedProduct && formData.quantity > 0 && formData.quantity <= maxQuantity && (
            <Box sx={{ p: 2, bgcolor: 'success.light', borderRadius: 1 }}>
              <Typography variant="body2" color="success.dark">
                Order Summary: {formData.quantity} × {selectedProduct.name} = 
                <strong> ${(selectedProduct.price * formData.quantity).toFixed(2)}</strong>
              </Typography>
            </Box>
          )}

          {formData.quantity > maxQuantity && (
            <Alert severity="warning">
              Quantity exceeds available stock. Maximum: {maxQuantity}
            </Alert>
          )}

          <Button
            type="submit"
            variant="contained"
            startIcon={<AddShoppingCart />}
            disabled={loading || !isFormValid}
            sx={{ justifySelf: 'start' }}
          >
            {loading ? 'Creating Order...' : 'Create Order'}
          </Button>
        </Box>
      </CardContent>
    </Card>
  );
}