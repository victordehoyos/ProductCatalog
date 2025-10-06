#!/bin/bash

echo "Suite de Pruebas de Concurrencia - ProductCatalog API"
echo "========================================================"

# Esperar a que la API esté lista
echo "Esperando a que la API esté disponible..."
until curl -s http://localhost:5000/api/Products > /dev/null; do
    sleep 2
done

echo "API está disponible"

# Crear productos de prueba para tests de eliminación
echo ""
echo "Creando productos de prueba para tests de eliminación..."
k6 run k6/setup-test-products.js

echo ""
echo "1. PRUEBA CRÍTICA: Eliminar Producto + Crear Órdenes Simultáneamente..."
k6 run k6/delete-product-race.js --out json=results/delete-race.json

echo ""
echo "2. Prueba de Race Condition en Órdenes..."
k6 run k6/order-race-condition.js --out json=results/order-race.json

echo ""
echo "3. Prueba de Ciclo de Vida Completo (Caos Controlado)..."
k6 run k6/full-lifecycle-race.js --out json=results/full-lifecycle.json

echo ""
echo "4. Prueba de Actualización Concurrente de Productos..."
k6 run k6/product-update-race.js --out json=results/product-update.json

echo ""
echo "5. Verificación de Consistencia Final..."
k6 run k6/consistency-verifier.js

echo ""
echo "========================================================"
echo "Todas las pruebas completadas"
echo "Resultados guardados en carpeta 'results/'"