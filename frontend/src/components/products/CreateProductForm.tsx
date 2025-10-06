'use client';

import { useState } from 'react';
import {
  Card,
  CardContent,
  TextField,
  Button,
  Box,
  Typography,
  Alert,
} from '@mui/material';
import { Add } from '@mui/icons-material';
import { CreateProductDto } from '@/types';
import { apiService } from '@/services/api';
import { getErrorMessage } from '@/lib/error-handler';

const initialFormData: CreateProductDto = {
  name: '',
  description: '',
  price: 0,
  stock: 0,
};

export default function CreateProductForm() {
  const [formData, setFormData] = useState<CreateProductDto>(initialFormData);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'price' || name === 'stock' ? Number(value) : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setSuccess(false);

    try {
      await apiService.createProduct(formData);
      setFormData(initialFormData);
      setSuccess(true);
      // En una app real, aquí refrescaríamos la lista de productos
      window.location.reload(); // Simple refresh for now
    } catch (err) {
        const errorMessage = getErrorMessage(err);
        setError(errorMessage);
        console.error('Error creating product:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Add New Product
        </Typography>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {success && (
          <Alert severity="success" sx={{ mb: 2 }}>
            Product created successfully!
          </Alert>
        )}

        <Box component="form" onSubmit={handleSubmit} sx={{ display: 'grid', gap: 2 }}>
          <TextField
            label="Product Name"
            name="name"
            value={formData.name}
            onChange={handleChange}
            required
            fullWidth
          />
          
          <TextField
            label="Description"
            name="description"
            value={formData.description}
            onChange={handleChange}
            multiline
            rows={3}
            fullWidth
          />
          
          <Box display="grid" gridTemplateColumns="1fr 1fr" gap={2}>
            <TextField
              label="Price"
              name="price"
              type="number"
              value={formData.price}
              onChange={handleChange}
              required
              inputProps={{ min: 0.01, step: 0.01 }}
            />
            
            <TextField
              label="Stock"
              name="stock"
              type="number"
              value={formData.stock}
              onChange={handleChange}
              required
              inputProps={{ min: 0 }}
            />
          </Box>

          <Button
            type="submit"
            variant="contained"
            startIcon={<Add />}
            disabled={loading}
            sx={{ justifySelf: 'start' }}
          >
            {loading ? 'Creating...' : 'Create Product'}
          </Button>
        </Box>
      </CardContent>
    </Card>
  );
}