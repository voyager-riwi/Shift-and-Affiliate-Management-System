document.addEventListener('DOMContentLoaded', () => {

    // --- Elementos del DOM ---
    const selectServiceDesk = document.getElementById('selectServiceDesk');
    const btnLlamar = document.getElementById('btnLlamarSiguiente');
    const turnoActualElem = document.getElementById('turnoActual');
    const afiliadoActualElem = document.getElementById('afiliadoActual');
    const listaEspera = document.getElementById('listaEspera');
    const historialBody = document.getElementById('historialHoy');

    // --- Conexión a SignalR ---
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/ticketHub")
        .build();

    connection.on("UpdateWaitingList", function () {
        console.log("SignalR: Recibida actualización de listas.");
        cargarFilaDeEspera();
        cargarHistorial();
    });

    connection.on("TicketCalled", function (ticketCode, serviceDeskId) {
        console.log(`SignalR: Turno ${ticketCode} llamado a la caja ${serviceDeskId}.`);
        turnoActualElem.textContent = ticketCode;
        afiliadoActualElem.textContent = `Llamando al afiliado...`;
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
            const response = await fetch(`/api/Tickets/next/${serviceDeskId}`, {
                method: 'POST'
            });

            if (response.status === 404) {
                turnoActualElem.textContent = '---';
                afiliadoActualElem.textContent = 'No hay tiquetes en espera.';
            } else if (!response.ok) {
                throw new Error(`Error del servidor: ${response.status}`);
            } else {
                const calledTicket = await response.json();
                console.log(`API: Tiquete ${calledTicket.ticketCode} llamado.`);
                // La UI se actualiza gracias a los eventos de SignalR
            }

        } catch (error) {
            console.error('Error al llamar el turno:', error);
            turnoActualElem.textContent = 'Error';
            afiliadoActualElem.textContent = 'Fallo al llamar turno.';
        } finally {
            setTimeout(() => { btnLlamar.disabled = false; }, 2000);
        }
    });

    // --- Funciones de Carga de Datos ---
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
                    const horaAtencion = ticket.servedAt ? new Date(ticket.servedAt).toLocaleTimeString('es-CO') : 'N/A';

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