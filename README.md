
# 🔌 PowerGate API – Documentación de Endpoints

API REST para monitoreo energético (ESP32). Aquí se describen los endpoints disponibles, junto con los parámetros esperados y la estructura de respuesta.

---

## 🔐 Auth

### `POST /api/Auth/login`
Inicia sesión del usuario.

**Parámetros (JSON Body):**
- `correo` (string): Correo electrónico del usuario.
- `contrasena` (string): Contraseña del usuario.

**Respuesta esperada:**
```json
{
  "data": { "correo": "usuario@example.com" },
  "isSuccess": true,
  "message": "Inicio de sesión exitoso."
}
```

---

### `POST /api/Auth/registro`
Registra un nuevo usuario.

**Parámetros (JSON Body):**
- `correo` (string): Correo electrónico.
- `contrasena` (string): Contraseña.

**Respuesta esperada:**
```json
{
  "data": {
    "correo": "example@gmail.com"
  },
  "isSuccess": true,
  "message": "Usuario registrado con éxito."
}
```

---

## 📟 Dispositivo

### `GET /api/Dispositivo/{dispositivoId}/estado`
Consulta el estado actual de un dispositivo.

**Parámetro de ruta:**
- `dispositivoId` (int): ID del dispositivo.

**Respuesta esperada:**
```json
{
  "dispositivoId": 1,
  "nombre": "dispositivo-1",
  "ubicacion": "Planta Baja - Sala",
  "potenciaTotal": 305.1,
  "energiaTotalSesion": 2.35,
  "tienePresencia": true,
  "canales": [
    {
      "canalId": 1,
      "nombre": "Canal A1",
      "potencia": 184.3,
      "releActivo": true
    },
    {
      "canalId": 2,
      "nombre": "Canal A2",
      "potencia": 120.8,
      "releActivo": false
    }
  ]
}
```

---

## ⚡ Lecturas

### `POST /api/Lecturas/EnviarLectura`
Registra una nueva lectura enviada por el ESP32.

**Parámetros (JSON Body):**
- `canalId` (int): ID del canal de carga.
- `timestamp` (datetime): Fecha y hora de la lectura.
- `voltaje` (decimal): Voltaje registrado.
- `corriente` (decimal): Corriente registrada.
- `potencia` (decimal): Potencia calculada.
- `energiaSesion` (decimal): Energía acumulada en la sesión.
- `presencia` (bool): Presencia detectada.
- `releActivo` (bool): Estado del relé.

**Respuesta esperada:**
```json
{
  "data": 150,
  "isSuccess": true,
  "message": "Lectura registrada correctamente."
}
```

---

### `GET /api/Lecturas`
Obtiene todas las lecturas registradas.

**Parámetros**: Ninguno

**Respuesta esperada:**
```json
{
  "data": [
    {
      "canalId": 1,
      "timestamp": "2025-07-24T18:15:00",
      "voltaje": 220.5,
      "corriente": 1.12,
      "potencia": 246.96,
      "energiaSesion": 0.123,
      "presencia": true,
      "releActivo": true
    }
  ],
  "isSuccess": true,
  "message": "Lecturas obtenidas correctamente."
}
```

---

**Formato de respuesta común (`Response<T>`):**
```json
{
  "data": T,
  "isSuccess": true/false,
  "message": "Descripción del resultado"
}
```
