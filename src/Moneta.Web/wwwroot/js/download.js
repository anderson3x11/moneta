window.monetaDownload = (fileName, contentType, base64) => {
    const link = document.createElement('a');
    link.href = `data:${contentType};base64,${base64}`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// Session persistence for the JWT so a page reload keeps the user signed in.
window.monetaSetSession = (token, email) => {
    localStorage.setItem('moneta_token', token);
    localStorage.setItem('moneta_email', email || '');
};
window.monetaGetToken = () => localStorage.getItem('moneta_token');
window.monetaGetEmail = () => localStorage.getItem('moneta_email');
window.monetaClearSession = () => {
    localStorage.removeItem('moneta_token');
    localStorage.removeItem('moneta_email');
};
