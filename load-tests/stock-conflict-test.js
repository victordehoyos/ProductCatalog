import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  scenarios: {
    stock_conflict: {
      executor: 'constant-arrival-rate',
      rate: 100, // Alta tasa para forzar conflictos
      timeUnit: '1s', 
      duration: '1m',
      preAllocatedVUs: 50,
      maxVUs: 200,
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<300'],
    http_req_failed: ['rate<0.05'],
  }
};

const BASE_URL = 'http://localhost:5000/api';

export default function () {
  const headers = {
    'Content-Type': 'application/json',
    'Authorization': 'Bearer test-token',
    'Idempotency-Key': `stock-test-${__VU}-${Date.now()}`
  };

  // Intentar crear orden para producto con stock limitado
  const payload = JSON.stringify({
    productId: 1, // Mismo producto para todos
    quantity: 2   // Cantidad que puede agotar stock
  });

  const orderResponse = http.post(`${BASE_URL}/orders`, payload, { headers });

  // Verificar manejo de conflictos
  check(orderResponse, {
    'Order handled correctly': (r) => 
      r.status === 201 || // Orden creada
      r.status === 400 || // Stock insuficiente
      r.status === 404,   // Producto no encontrado
    'Fast conflict resolution': (r) => r.timings.duration < 500,
    'Clear error message when failed': (r) => 
      r.status === 201 || (r.status >= 400 && r.body.length > 0)
  });

  // Estadísticas específicas por código de respuesta
  if (orderResponse.status === 201) {
    console.log(`Orden creada exitosamente - VU: ${__VU}`);
  } else if (orderResponse.status === 400) {
    console.log(`Sin stock - VU: ${__VU}`);
  }

  sleep(0.05); // Mínima pausa para máximo estrés
}