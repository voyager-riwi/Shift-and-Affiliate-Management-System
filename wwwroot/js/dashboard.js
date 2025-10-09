document.addEventListener('DOMContentLoaded', () => {

    // --- Referencias a los Elementos del DOM ---
    const selectServiceDesk = document.getElementById('selectServiceDesk');
    const btnLlamarSiguiente = document.getElementById('btnLlamarSiguiente');
    const turnoActualDiv = document.getElementById('turnoActual');
    const afiliadoActualDiv = document.getElementById('afiliadoActual');
    const cardAtendiendo = document.getElementById('cardAtendiendo');
    const listaEsperaUl = document.getElementById('listaEspera');
    const historialHoyTbody = document.getElementById('historialHoy');

    // --- Conexión a SignalR ---
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/queueHub")
        .withAutomaticReconnect()
        .build();

    // --- Funciones para Renderizar la UI ---

    // Dibuja la lista de tiquetes en espera
    function renderWaitingList(tickets) {
        listaEsperaUl.innerHTML = '';
        if (!tickets || tickets.length === 0) {
            listaEsperaUl.innerHTML = '<li class="p-4 text-gray-500 text-center">No hay turnos en espera.</li>';
            return;
        }
        tickets.forEach(ticket => {
            const li = document.createElement('li');
            li.className = 'list-item-animation p-4 flex justify-between items-center';
            li.innerHTML = `
                <div>
                    <span class="font-oswald text-2xl font-medium text-gray-800">${ticket.ticketCode}</span>
                    <span class="block text-xs text-gray-500">${ticket.affiliate?.fullName ?? 'Visitante'}</span>
                </div>
                <span class="text-sm font-semibold text-gray-400">${new Date(ticket.createdAt).toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' })}</span>
            `;
            listaEsperaUl.appendChild(li);
        });
    }

    // Dibuja la tabla del historial de hoy
    function renderHistoryTable(tickets) {
        historialHoyTbody.innerHTML = '';
        if (!tickets || tickets.length === 0) {
            historialHoyTbody.innerHTML = '<tr><td colspan="3" class="px-6 py-4 text-center text-sm text-gray-500">No hay turnos atendidos hoy.</td></tr>';
            return;
        }
        tickets.forEach(ticket => {
            const tr = document.createElement('tr');
            tr.className = 'list-item-animation';

            const horaAtencion = ticket.servedAt
                ? new Date(ticket.servedAt).toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
                : 'N/A';

            tr.innerHTML = `
                <td class="px-6 py-4 whitespace-nowrap">
                    <span class="font-oswald text-lg font-medium text-gray-900">${ticket.ticketCode}</span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${horaAtencion}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-700">${ticket.affiliate?.fullName ?? 'Visitante'}</td>
            `;
            historialHoyTbody.appendChild(tr);
        });
    }

    // Actualiza la tarjeta de "Atendiendo Ahora"
    function updateAtendiendoCard(ticket) {
        if (ticket) {
            turnoActualDiv.textContent = ticket.ticketCode;
            afiliadoActualDiv.textContent = ticket.affiliate?.fullName ?? 'Visitante';

            // ✅ VERIFICAR QUE EL ELEMENTO EXISTE ANTES DE MANIPULARLO
            if (cardAtendiendo) {
                cardAtendiendo.classList.add('active');
                setTimeout(() => {
                    cardAtendiendo.classList.remove('active');
                }, 2000);
            }
        } else {
            turnoActualDiv.textContent = '- - -';
            afiliadoActualDiv.textContent = 'Esperando turno...';
            if (cardAtendiendo) {
                cardAtendiendo.classList.remove('active');
            }
        }
    }

    // --- Lógica de Carga Inicial ---
    async function loadInitialData() {
        try {
            const [waitingResponse, historyResponse] = await Promise.all([
                fetch('/api/Tickets'),
                fetch('/api/Tickets/history/today')
            ]);

            if (!waitingResponse.ok || !historyResponse.ok) {
                throw new Error('Error al cargar los datos');
            }

            const waitingTickets = await waitingResponse.json();
            const historyTickets = await historyResponse.json();

            renderWaitingList(waitingTickets);
            renderHistoryTable(historyTickets);

            console.log('✅ Dashboard: Datos iniciales cargados');
        } catch (error) {
            console.error("❌ Error al cargar los datos iniciales:", error);
            listaEsperaUl.innerHTML = '<li class="p-4 text-red-500 text-center">Error al cargar la fila de espera.</li>';
            historialHoyTbody.innerHTML = '<tr><td colspan="3" class="px-6 py-4 text-center text-red-500">Error al cargar el historial.</td></tr>';
        }
    }

    // Recargar solo el historial
    async function reloadHistory() {
        try {
            const response = await fetch('/api/Tickets/history/today');
            if (!response.ok) throw new Error('Error al cargar historial');

            const historyTickets = await response.json();
            renderHistoryTable(historyTickets);
            console.log('🔄 Dashboard: Historial actualizado');
        } catch (error) {
            console.error("❌ Error al recargar el historial:", error);
        }
    }

    // --- Event Listeners ---

    // Botón "Llamar Siguiente"
    if (btnLlamarSiguiente) {
        btnLlamarSiguiente.addEventListener('click', async () => {
            const serviceDeskId = selectServiceDesk.value;
            if (!serviceDeskId) {
                alert('Por favor, seleccione un puesto de atención.');
                return;
            }

            btnLlamarSiguiente.disabled = true;
            btnLlamarSiguiente.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i> Llamando...';

            try {
                const response = await fetch(`/api/Tickets/next/${serviceDeskId}`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                });

                if (response.status === 404) {
                    alert('No hay más turnos en la fila de espera.');
                    updateAtendiendoCard(null);
                } else if (!response.ok) {
                    throw new Error('Error en el servidor al llamar al turno.');
                } else {
                    const calledTicket = await response.json();
                    console.log('📞 Dashboard: Turno llamado localmente', calledTicket);
                    updateAtendiendoCard(calledTicket);
                }
            } catch (error) {
                console.error("❌ Error al llamar al siguiente turno:", error);
                alert('Ocurrió un error al intentar llamar al turno.');
            } finally {
                btnLlamarSiguiente.disabled = false;
                btnLlamarSiguiente.innerHTML = '<i class="fas fa-phone-volume mr-2"></i> Llamar Siguiente';
            }
        });
    }

    // --- Handlers de SignalR ---

    connection.on("ReceiveNewCall", (ticket) => {
        console.log('📢 Dashboard: Recibido nuevo llamado por SignalR', ticket);
        updateAtendiendoCard(ticket);
        setTimeout(() => reloadHistory(), 500);
    });

    connection.on("UpdateWaitingList", (waitingTickets) => {
        console.log('📋 Dashboard: Lista de espera actualizada', waitingTickets);
        renderWaitingList(waitingTickets);
    });

    // ✅ NUEVO: Escuchar el evento de reinicio
    connection.on("SystemReset", () => {
        console.log('🔄 Dashboard: Sistema reiniciado');
        // Limpiar todas las listas
        renderWaitingList([]);
        renderHistoryTable([]);
        updateAtendiendoCard(null);
    });

    // --- Iniciar Conexión ---
    async function start() {
        try {
            await connection.start();
            console.log("✅ SignalR conectado (Dashboard)");
            await loadInitialData();
        } catch (err) {
            console.error("❌ Error de conexión SignalR:", err);
            setTimeout(start, 5000);
        }
    }

    connection.onreconnected(() => {
        console.log("🔄 SignalR reconectado");
        loadInitialData();
    });

    start();
});