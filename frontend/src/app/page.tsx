import { Container, Typography, Box } from '@mui/material';
import ProductList from '@/components/products/ProductList';
import CreateProductForm from '@/components/products/CreateProductForm';

export default function HomePage() {
  return (
    <Container maxWidth="xl">
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Product Catalog
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Manage your products and inventory
        </Typography>
      </Box>

      <Box sx={{ display: 'grid', gap: 4 }}>
        <CreateProductForm />
        <ProductList />
      </Box>
    </Container>
  );
}