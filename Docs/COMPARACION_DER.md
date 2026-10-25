# Modelo implementado

Se conservan las seis entidades: USUARIO, ESPACIO, RESERVA, EQUIPAMIENTO, INCLUYE y PAGO. USUARIO y ESPACIO tienen relaciones uno a muchos con RESERVA; INCLUYE resuelve la relación muchos a muchos con EQUIPAMIENTO y conserva cantidad y precio histórico.

Una reserva tiene cero o un pago: antes de pagar no existe un registro de PAGO. La pantalla de pagos pendientes consulta las reservas sin pago. Vencida es una condición calculada para una reserva pendiente cuyo inicio ya pasó; no requiere cambiar el enum almacenado.

Se agregan campos funcionales como nombres, códigos únicos, folio, credenciales cifradas mediante hash y costo total. ESPACIO incluye foto opcional; PAGO incluye comprobante opcional. Estas dos columnas se agregan al iniciar si faltan, sin eliminar registros existentes.

El archivo DER_implementado.mmd describe las columnas actuales. DER_original.png conserva el esquema recibido. Los archivos de fotos y comprobantes se respaldan junto con la base; las columnas almacenan su ubicación, no los bytes.

USUARIO incluye ahora rol (1=Admin, 2=Cliente), incorporado al unificar las ramas. Los permisos de sesión se obtienen de ese atributo. El DER editable actualizado está en DER_implementado.drawio.
