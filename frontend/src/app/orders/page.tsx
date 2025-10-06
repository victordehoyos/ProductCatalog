import CreateOrderForm from '@/components/orders/CreateOrderForm';
import OrderList from '@/components/orders/OrderList';
import { Container, Typography, Box } from '@mui/material';

export default function OrdersPage() {
  return (
    <Container maxWidth="xl">
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Order Management
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Create and manage purchase orders
        </Typography>
      </Box>

      <Box sx={{ display: 'grid', gap: 4 }}>
        <CreateOrderForm />
        <OrderList />
      </Box>
    </Container>
  );
}