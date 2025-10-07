document.addEventListener('DOMContentLoaded', () => {

    // --- Elementos del DOM ---
    const selectServiceDesk = document.getElementById('selectServiceDesk');
    const btnLlamar = document.getElementById('btnLlamarSiguiente');
    const turnoActualElem = document.getElementById('turnoActual');
    const afiliadoActualElem = document.getElementById('afiliadoActual');
    const listaEspera = document.getElementById('listaEspera');
    const historialBody = document.getElementById('historialHoy');

    // --- Conexión a SignalR ---
    // CORRECCIÓN 1: Conexión al Hub de la cola mapeado en Program.cs
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/queueHub")
        .build();

    // Evento para forzar la recarga cuando hay cambios en otros puestos (se debe emitir desde el servidor)
    connection.on("UpdateWaitingList", function () {
        console.log("SignalR: Recibida actualización de listas.");
        cargarFilaDeEspera();
        cargarHistorial();
    });

    // CORRECCIÓN 2: El Dashboard NO necesita el evento de llamado si actualiza por la respuesta AJAX.
    // Lo mantenemos por si el Dashboard quiere escuchar llamados de OTROS puestos.
    connection.on("ReceiveNewCall", function (ticketCode, deskNumber) {
        console.log(`SignalR: Turno ${ticketCode} llamado al puesto ${deskNumber} (Por otro operador).`);
        // Llama a las funciones para refrescar las listas si es un llamado que no hice yo.
        cargarFilaDeEspera();
        cargarHistorial();
    });

    connection.start().then(function () {
        console.log("Conectado a SignalR. Cargando datos iniciales...");
        cargarFilaDeEspera();
        cargarHistorial();
    }).catch(function (err) {
        console.error("Error de SignalR:", err.toString());
    });


    // --- Lógica de la Página (Funcionalidad del Botón) ---
    btnLlamar.addEventListener('click', async () => {
        const serviceDeskId = selectServiceDesk.value;

        if (!serviceDeskId) {
            alert("Por favor, seleccione un puesto de atención.");
            return;
        }

        btnLlamar.disabled = true;
        turnoActualElem.textContent = 'Llamando...';
        afiliadoActualElem.textContent = 'Buscando siguiente turno...';

        try {
            // CORRECCIÓN 3: Llamada al QueueController que tiene la lógica de SignalR
            const response = await fetch(`/Queue/CallNext`, {
                method: 'POST',
                // Debe usar URLSearchParams para enviar datos como FromForm
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: `serviceDeskId=${serviceDeskId}`
            });

            if (response.status === 404) {
                const error = await response.json();
                turnoActualElem.textContent = '---';
                afiliadoActualElem.textContent = error.message; // Muestra el mensaje del servidor
            } else if (!response.ok) {
                const error = await response.json();
                throw new Error(`Error del servidor: ${error.message || response.statusText}`);
            } else {
                // CORRECCIÓN 4: Usar la respuesta JSON del servidor para actualizar la UI local
                const calledTicket = await response.json();

                turnoActualElem.textContent = calledTicket.ticketCode;
                afiliadoActualElem.textContent = calledTicket.afiliado;

                console.log(`API: Tiquete ${calledTicket.ticketCode} llamado. Señal enviada.`);

                // Forzar una recarga visual de las listas inmediatamente (aunque SignalR también lo hará)
                cargarFilaDeEspera();
                cargarHistorial();
            }

        } catch (error) {
            console.error('Error al llamar el turno:', error);
            turnoActualElem.textContent = 'Error';
            afiliadoActualElem.textContent = 'Fallo al llamar turno.';
        } finally {
            // Habilitar el botón después de un breve retraso para evitar doble clic
            setTimeout(() => { btnLlamar.disabled = false; }, 1000);
        }
    });

    // --- Funciones de Carga de Datos (Permanecen sin cambios estructurales) ---

    // (Tu código de cargarFilaDeEspera y cargarHistorial va aquí, no necesita cambios)

    async function cargarFilaDeEspera() {
        try {
            const response = await fetch('/api/Tickets');
            if (!response.ok) throw new Error('Respuesta no válida de la API.');

            const tickets = await response.json();
            listaEspera.innerHTML = '';

            if (tickets.length === 0) {
                listaEspera.innerHTML = '<li class="list-group-item">No hay tiquetes en espera.</li>';
            } else {
                tickets.forEach(ticket => {
                    const li = document.createElement('li');
                    li.className = 'list-group-item';
                    li.textContent = ticket.ticketCode;
                    listaEspera.appendChild(li);
                });
            }

        } catch (error) {
            console.error("Error cargando fila de espera:", error);
            listaEspera.innerHTML = '<li class="list-group-item text-danger">Error al cargar la fila.</li>';
        }
    }

    async function cargarHistorial() {
        try {
            const response = await fetch('/api/Tickets/history/today');
            if (!response.ok) throw new Error('Respuesta no válida de la API.');

            const tickets = await response.json();
            historialBody.innerHTML = '';

            if (tickets.length === 0) {
                historialBody.innerHTML = '<tr><td colspan="3">No hay historial para hoy.</td></tr>';
            } else {
                tickets.forEach(ticket => {
                    const tr = document.createElement('tr');
                    // Cambiado de 'es-CO' a 'es-ES' por mayor compatibilidad, puedes ajustarlo si necesitas el formato de Colombia.
                    const horaAtencion = ticket.servedAt ? new Date(ticket.servedAt).toLocaleTimeString('es-ES') : 'N/A';

                    // CORRECCIÓN: Si el afiliado es nulo, muestra 'Anónimo'
                    const affiliateName = ticket.affiliate ? ticket.affiliate.fullName : 'Anónimo';

                    tr.innerHTML = `
                        <td>${ticket.ticketCode}</td>
                        <td>${horaAtencion}</td>
                        <td>${affiliateName}</td>
                    `;
                    historialBody.appendChild(tr);
                });
            }

        } catch (error) {
            console.error("Error cargando historial:", error);
            historialBody.innerHTML = '<tr><td colspan="3" class="text-danger">Error al cargar el historial.</td></tr>';
        }
    }
});
