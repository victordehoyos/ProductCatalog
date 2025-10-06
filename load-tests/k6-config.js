// k6-config.js
export const options = {
  scenarios: {
    // Escenario de carga constante
    constant_load: {
      executor: 'constant-arrival-rate',
      rate: 50, // 50 solicitudes por segundo
      timeUnit: '1s',
      duration: '2m',
      preAllocatedVUs: 20,
      maxVUs: 100,
    },
    
    // Escenario de picos
    spike_test: {
      executor: 'ramping-arrival-rate',
      startRate: 10,
      timeUnit: '1s',
      stages: [
        { target: 100, duration: '30s' },  // Rampa up
        { target: 100, duration: '1m' },   // Mantener pico
        { target: 10, duration: '30s' },   // Rampa down
      ],
      preAllocatedVUs: 10,
      maxVUs: 150,
    },
    
    // Escenario de estrés
    stress_test: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 50 },
        { duration: '1m', target: 100 },
        { duration: '30s', target: 200 },
        { duration: '30s', target: 0 },
      ],
    }
  },
  
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% de requests bajo 500ms
    http_req_failed: ['rate<0.01'],   // Menos del 1% de errores
    checks: ['rate>0.95']             // 95% de checks pasan
  }
};