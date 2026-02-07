// Функція для завантаження файлів з браузера
window.downloadFile = (fileName, base64String, mimeType) => {
    // Конвертуємо base64 в blob
    const byteCharacters = atob(base64String);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: mimeType });

    // Створюємо URL для blob
    const url = window.URL.createObjectURL(blob);

    // Створюємо тимчасовий елемент <a> для завантаження
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;

    // Додаємо до DOM, клікаємо та видаляємо
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    // Звільняємо пам'ять
    window.URL.revokeObjectURL(url);
};