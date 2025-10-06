'use client';

import { useState, useEffect } from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  Alert,
  CircularProgress,
  Chip,
} from '@mui/material';
import { Receipt } from '@mui/icons-material';
import { Order } from '@/types';
import { apiService } from '@/services/api';

export default function OrderList() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchOrders = async () => {
    try {
      setLoading(true);
      const data = await apiService.getOrders();
      setOrders(data);
      setError(null);
    } catch (err) {
      setError('Failed to fetch orders');
      console.error('Error fetching orders:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrders();
  }, []);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight={200}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        {error}
      </Alert>
    );
  }

  return (
    <Box>
      <Typography variant="h5" gutterBottom>
        Recent Orders ({orders.length})
      </Typography>

      {orders.length === 0 ? (
        <Card>
          <CardContent>
            <Box textAlign="center" py={4}>
              <Receipt sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
              <Typography variant="h6" color="text.secondary">
                No orders found
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Create your first order to get started
              </Typography>
            </Box>
          </CardContent>
        </Card>
      ) : (
        <Box sx={{ display: 'grid', gap: 2 }}>
          {orders.map((order) => (
            <Card key={order.id}>
              <CardContent>
                <Box display="flex" justifyContent="space-between" alignItems="flex-start">
                  <Box>
                    <Typography variant="h6" gutterBottom>
                      {order.productName}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Quantity: {order.quantity}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Date: {new Date(order.date).toLocaleDateString()}
                    </Typography>
                  </Box>
                  <Box textAlign="right">
                    <Chip 
                      label={`$${order.total.toFixed(2)}`} 
                      color="primary" 
                      variant="outlined"
                    />
                  </Box>
                </Box>
              </CardContent>
            </Card>
          ))}
        </Box>
      )}
    </Box>
  );
}