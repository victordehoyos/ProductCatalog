// order-concurrency-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';
import { options } from './k6-config.js';

export { options };

const BASE_URL = 'http://localhost:5000/api';
let productId = 1;
let orderCounter = 0;

// Función para generar token JWT (simulado)
function getAuthToken() {
  // En un caso real, harías login primero o usarías un token pre-generado
  return 'test-jwt-token';
}

export default function () {
  const headers = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${getAuthToken()}`,
    'Idempotency-Key': `test-key-${__VU}-${Date.now()}-${orderCounter++}`
  };

  // Escenario 1: Crear órdenes concurrentes para el mismo producto
  const createOrderPayload = JSON.stringify({
    productId: productId,
    quantity: 1
  });

  const createOrderRes = http.post(`${BASE_URL}/orders`, createOrderPayload, { headers });
  
  check(createOrderRes, {
    'Order creation status is 201 or 400 (concurrency handling)': (r) => 
      r.status === 201 || r.status === 400,
    'Order creation response time OK': (r) => r.timings.duration < 1000,
  });

  // Si la orden se creó exitosamente, verificar el stock
  if (createOrderRes.status === 201) {
    const checkStockRes = http.get(`${BASE_URL}/products/${productId}`, { headers });
    
    check(checkStockRes, {
      'Stock check successful': (r) => r.status === 200,
      'Stock is consistent': (r) => {
        if (r.status === 200) {
          const product = JSON.parse(r.body);
          return product.stock >= 0; // Stock nunca negativo
        }
        return false;
      }
    });
  }

  sleep(0.1); // Pequeña pausa entre requests
}