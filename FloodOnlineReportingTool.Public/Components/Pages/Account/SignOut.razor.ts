function clearAndRedirect() {
    sessionStorage.clear();
    const returnUrl = new URLSearchParams(window.location.search).get('returnUrl') || '/';
    window.location.href = returnUrl;
}
window.addEventListener('DOMContentLoaded', clearAndRedirect);