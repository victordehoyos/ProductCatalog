import http from 'k6/http';
import { check, sleep } from 'k6';
import { options } from './k6-config.js';

export { options };

const BASE_URL = 'http://localhost:5000/api';

function getAuthToken() {
  return 'test-jwt-token';
}

export default function () {
  const headers = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${getAuthToken()}`
  };

  const vuId = __VU; // Virtual User ID
  const timestamp = Date.now();
  
  // Operaciones concurrentes en productos
  const operations = [
    // 1. Obtener lista de productos
    () => http.get(`${BASE_URL}/products`, { headers }),
    
    // 2. Obtener producto específico
    () => http.get(`${BASE_URL}/products/1`, { headers }),
    
    // 3. Actualizar producto
    () => {
      const updatePayload = JSON.stringify({
        name: `Updated Product ${vuId}-${timestamp}`,
        description: `Concurrent update ${vuId}`,
        price: 99.99
      });
      return http.put(`${BASE_URL}/products/1`, updatePayload, { headers });
    },
    
    // 4. Aumentar stock
    () => http.post(`${BASE_URL}/products/1/increase-stock?qty=1`, null, { headers }),
    
    // 5. Crear nuevo producto (si es posible)
    () => {
      const newProductPayload = JSON.stringify({
        name: `Concurrent Product ${vuId}-${timestamp}`,
        description: 'Created under load',
        price: 50.00,
        stock: 10
      });
      return http.post(`${BASE_URL}/products`, newProductPayload, { headers });
    }
  ];

  // Ejecutar operación aleatoria
  const randomOp = operations[Math.floor(Math.random() * operations.length)];
  const response = randomOp();

  // Verificaciones comunes
  check(response, {
    'Request successful': (r) => r.status >= 200 && r.status < 300,
    'Response time acceptable': (r) => r.timings.duration < 800,
    'Has valid JSON response': (r) => {
      try {
        JSON.parse(r.body);
        return true;
      } catch {
        return r.status === 204 || r.body === ''; // No Content es válido
      }
    }
  });

  sleep(0.2);
}