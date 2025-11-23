const users = [
    { name: "Admin", login: "admin", pass: "123", role: "Administrator" },
    { name: "Manager", login: "manager", pass: "123", role: "Manager" },
    { name: "Operator", login: "operator", pass: "123", role: "Operator" }
];

let inventory = [
    { art: "100-AB", name: "Сенсор IoT (A)", qty: 150, loc: "A-12" },
    { art: "101-BC", name: "RFID-мітка (B)", qty: 5000, loc: "Б-04" },
    { art: "203-CD", name: "Сканер (X)", qty: 45, loc: "A-02" },
    { art: "305-EF", name: "Сервер Rack 4U", qty: 10, loc: "C-01" },
    { art: "401-XX", name: "Кабель Eth 50m", qty: 200, loc: "D-10" },
    { art: "550-PO", name: "Блок живлення", qty: 30, loc: "A-05" }
];

function switchScreen(screenId, title) {
    document.querySelectorAll('.screen').forEach(s => s.classList.remove('active'));
    document.getElementById(screenId).classList.add('active');
    document.getElementById('windowTitle').innerText = "WIMS - " + title;
    const win = document.getElementById('appWindow');
    win.style.width = (screenId === 'managerScreen') ? "650px" : "400px";
}

function showToast(message) {
    const toast = document.getElementById('notification-overlay');
    toast.innerText = message;
    toast.classList.add('show');
    setTimeout(() => toast.classList.remove('show'), 2500);
}

function attemptLogin() {
    const l = document.getElementById('loginUser').value.trim();
    const p = document.getElementById('loginPass').value.trim();
    const user = users.find(u => u.login === l && u.pass === p);
    if (user) {
        if (user.role === "Operator") {
            document.getElementById('operatorWelcome').innerText = `Вітаємо, ${user.name}!`;
            switchScreen('operatorScreen', 'Панель оператора');
        } else if (user.role === "Manager") {
            renderGrid();
            switchScreen('managerScreen', 'Панель менеджера');
        } else if (user.role === "Administrator") {
            switchScreen('adminScreen', 'Адміністратор');
        }
    } else {
        showToast("❌ Невірний логін або пароль!");
    }
}

function logout() {
    document.getElementById('loginUser').value = "";
    document.getElementById('loginPass').value = "";
    switchScreen('loginScreen', 'Вхід');
}

function processReceive() {
    const art = document.getElementById('recvArt').value;
    const qty = parseInt(document.getElementById('recvQty').value);
    const item = inventory.find(i => i.art === art);
    
    if (item && qty > 0) {
        item.qty += qty;
        showToast(`✅ Прийнято ${qty} од. товару ${art}. Новий залишок: ${item.qty}`);
        document.getElementById('recvArt').value = "";
        document.getElementById('recvQty').value = "";
        switchScreen('operatorScreen', 'Панель Оператора');
    } else if (!item) {
        showToast("❌ Товар з таким артикулом не знайдено!");
    } else {
        showToast("❌ Введіть коректну кількість!");
    }
}

function processShip() {
    const art = document.getElementById('shipArt').value;
    const qty = parseInt(document.getElementById('shipQty').value);
    const item = inventory.find(i => i.art === art);

    if (item && qty > 0) {
        if (item.qty >= qty) {
            item.qty -= qty;
            showToast(`📦 Відвантажено ${qty} од. товару ${art}. Залишок: ${item.qty}`);
            document.getElementById('shipArt').value = "";
            document.getElementById('shipQty').value = "";
            switchScreen('operatorScreen', 'Панель Оператора');
        } else {
            showToast(`⚠️ Недостатньо товару! Доступно: ${item.qty}`);
        }
    } else {
        showToast("❌ Помилка вводу або товар не знайдено!");
    }
}

function processCheck() {
    const art = document.getElementById('checkArt').value;
    const item = inventory.find(i => i.art === art);
    if (item) {
        showToast(`ℹ️ Інфо: ${item.name} | К-сть: ${item.qty} | Місце: ${item.loc}`);
    } else {
        showToast("❌ Товар не знайдено!");
    }
}

function processTransfer() {
    const art = document.getElementById('transArt').value;
    const loc = document.getElementById('transLoc').value;
    const qty = parseInt(document.getElementById('transQty').value);
    const item = inventory.find(i => i.art === art);

    if (item && loc && qty > 0) {
        if (qty <= item.qty) {
             item.loc = loc;
             showToast(`🔄 Товар ${art} (${qty} шт.) переміщено в ${loc}`);
             switchScreen('operatorScreen', 'Панель Оператора');
        } else {
             showToast(`⚠️ Недостатньо товару для переміщення!`);
        }
    } else {
        showToast("❌ Заповніть всі поля коректно!");
    }
}

function renderGrid(filter = "") {
    const tbody = document.querySelector('#inventoryTable tbody');
    tbody.innerHTML = "";
    inventory.forEach(item => {
        if (item.name.toLowerCase().includes(filter.toLowerCase()) || item.art.toLowerCase().includes(filter.toLowerCase())) {
            tbody.innerHTML += `<tr><td class="font-mono text-xs">${item.art}</td><td>${item.name}</td><td class="text-center">${item.qty}</td><td class="text-gray-500 text-sm">${item.loc}</td></tr>`;
        }
    });
}

function filterGrid() { renderGrid(document.getElementById('searchInput').value); }

function saveNewItem() {
    const art = document.getElementById('newItemArt').value;
    const name = document.getElementById('newItemName').value;
    const qty = document.getElementById('newItemQty').value;
    const loc = document.getElementById('newItemLoc').value;
    if(!art || !name || !qty) { showToast("⚠️ Заповніть поля!"); return; }
    inventory.push({ art: art, name: name, qty: parseInt(qty), loc: loc });
    showToast(`✅ Товар ${art} додано!`);
    renderGrid();
    switchScreen('managerScreen', 'Панель менеджера');
}

function generateExcel() {
    let csvContent = "Артикул,Назва товару,Кількість,Розташування\n" + inventory.map(e => `${e.art},${e.name},${e.qty},${e.loc}`).join("\n");
    const link = document.createElement("a");
    link.href = URL.createObjectURL(new Blob(["\ufeff" + csvContent], { type: 'text/csv;charset=utf-8;' }));
    link.download = "WIMS_Report.csv";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    showToast("📊 Файл звіту завантажено!");
}

function createUser() {
    const fio = document.getElementById('newFio').value;
    const login = document.getElementById('newLogin').value;
    if(!fio || !login) { showToast("⚠️ Заповніть всі поля!"); return; }
    const role = document.querySelector('input[name="role"]:checked').value;
    users.push({ name: fio, login: login, pass: "123", role: role });
    showToast(`✅ Користувача ${login} створено!`);
    switchScreen('loginScreen', 'Вхід');
}