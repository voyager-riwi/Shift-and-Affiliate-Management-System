// Variable global para almacenar los datos del afiliado
let currentAffiliate = null;

// --- Funciones de Utilidad ---

// 1. Mostrar mensajes/alertas personalizados
function showAlert(title, message, isError = false) {
    const alertBox = document.getElementById('customAlert');
    document.getElementById('alertTitle').textContent = title;
    document.getElementById('alertMessage').textContent = message;

    alertBox.classList.remove('hidden', 'bg-green-100', 'bg-red-100');
    if (isError) {
        alertBox.classList.add('bg-red-100', 'text-red-800');
    } else {
        alertBox.classList.add('bg-green-100', 'text-green-800');
    }

    alertBox.classList.remove('hidden');
    setTimeout(() => { alertBox.classList.add('hidden'); }, 5000);
}

// 2. Función clave para la IMPRESIÓN AUTOMÁTICA
function imprimirTiquete(ticketId) {
    const url = `/Print/Ticket/${ticketId}`;

    // Abre una ventana pequeña. Esto abrirá el diálogo de impresión.
    const printWindow = window.open(url, 'ImpresionTiquete', 'width=300,height=300,left=100,top=100');

    if (printWindow) {
        // Enfoca la nueva ventana para asegurar que el diálogo aparezca.
        printWindow.focus();

        // Cierra la ventana automáticamente 2 segundos después.
        // Esto da tiempo al sistema operativo para capturar el trabajo de impresión.
        setTimeout(() => {
            try {
                printWindow.close();
            } catch (e) {
                console.warn("No se pudo cerrar la ventana de impresión automáticamente.");
            }
        }, 2000);
    }
}


// --- LÓGICA DE GENERACIÓN DE TURNO ---

async function generarTurno() {
    if (currentAffiliate === null) return;

    // Referencias del DOM
    const mensajeTurno = document.getElementById('mensajeTurno');
    const btnPedirTurno = document.getElementById('btnPedirTurno');
    const btnAnonymousTurn = document.getElementById('btnAnonymousTurn');

    // Deshabilitar botones para evitar doble clic
    btnPedirTurno.disabled = true;
    btnAnonymousTurn.disabled = true;

    try {
        // Preparar la llamada a la API
        const docId = currentAffiliate.documentId || '';
        // Endpoint: /api/Tickets/{documentId} o /api/Tickets/
        const url = `/api/Tickets/${docId}`;

        mensajeTurno.innerHTML = '<p class="text-xl text-gray-700 font-semibold mt-4"><i class="fas fa-spinner fa-spin mr-2"></i> Procesando su solicitud...</p>';

        const respuesta = await fetch(url, { method: 'POST' });

        if (!respuesta.ok) {
            throw new Error('Error de servicio al crear el tiquete.');
        }

        const nuevoTiquete = await respuesta.json();

        // 1. Mostrar mensaje de éxito con el código del turno
        mensajeTurno.innerHTML = `<p class="text-3xl text-green-600 font-extrabold">¡TURNO GENERADO!</p>
                                  <p class="text-7xl font-black text-blue-600 my-4">${nuevoTiquete.ticketCode}</p>
                                  <p class="text-xl text-gray-700">El sistema está imprimiendo su tiquete. Recuerde presionar 'Imprimir' en el diálogo.</p>`;

        // 2. IMPRESIÓN AUTOMÁTICA
        imprimirTiquete(nuevoTiquete.id);

    } catch (error) {
        console.error('Error al pedir el turno:', error);
        showAlert('Error Grave', 'No se pudo generar su turno. Intente nuevamente.', true);
        mensajeTurno.textContent = '';
    } finally {
        // Reinicia el kiosko después de 10 segundos para dejarlo listo
        setTimeout(() => { location.reload(); }, 10000);
    }
}


// --- INICIALIZACIÓN Y EVENTOS DEL DOM ---

document.addEventListener('DOMContentLoaded', () => {
    // Referencias del DOM
    const initialScreen = document.getElementById('initialScreen');
    const searchSection = document.getElementById('searchSection');
    const ticketSection = document.getElementById('ticketSection');

    const btnShowSearch = document.getElementById('btnShowSearch');
    const btnAnonymousTurn = document.getElementById('btnAnonymousTurn');
    const btnBuscar = document.getElementById('btnBuscar');
    const btnVolver = document.getElementById('btnVolver');
    const inputDocumento = document.getElementById('inputDocumento');

    // --- MANEJO DE VISTAS ---
    btnShowSearch.addEventListener('click', () => {
        initialScreen.classList.add('hidden');
        searchSection.classList.remove('hidden');
        inputDocumento.focus();
    });

    btnVolver.addEventListener('click', () => {
        initialScreen.classList.remove('hidden');
        searchSection.classList.add('hidden');
        inputDocumento.value = '';
    });

    // --- BOTÓN TURNO ANÓNIMO ---
    btnAnonymousTurn.addEventListener('click', () => {
        currentAffiliate = { id: null, fullName: 'Visitante Anónimo' };
        generarTurno();
    });

    // --- BOTÓN PEDIR TURNO (después de buscar) ---
    // Este botón existe dentro de ticketSection
    const btnPedirTurno = document.getElementById('btnPedirTurno');
    if (btnPedirTurno) {
        btnPedirTurno.addEventListener('click', generarTurno);
    }


    // --- BOTÓN BUSCAR AFILIADO ---
    btnBuscar.addEventListener('click', async () => {
        const docId = inputDocumento.value.trim();
        if (!docId) {
            showAlert('Documento Requerido', 'Por favor, ingrese su número de documento.', true);
            return;
        }

        btnBuscar.disabled = true;

        try {
            // Endpoint: /api/Affiliates/ByDocument/{docId}
            const response = await fetch(`/api/Affiliates/ByDocument/${docId}`);

            if (response.status === 404) {
                showAlert('Afiliado No Encontrado', 'Su documento no fue encontrado. Obtendrá un turno como Visitante.', true);
                currentAffiliate = { id: null, fullName: 'Visitante', documentId: docId };
            } else if (!response.ok) {
                throw new Error('Error en la búsqueda del afiliado.');
            } else {
                currentAffiliate = await response.json();
            }

            // Pasar a la sección de confirmación
            document.getElementById('welcomeMessage').textContent = `Hola, ${currentAffiliate.fullName}`;
            searchSection.classList.add('hidden');
            ticketSection.classList.remove('hidden');

        } catch (error) {
            console.error('Error buscando afiliado:', error);
            showAlert('Error de Conexión', 'No se pudo completar la búsqueda. Intente nuevamente.', true);
        } finally {
            btnBuscar.disabled = false;
        }
    });
});
