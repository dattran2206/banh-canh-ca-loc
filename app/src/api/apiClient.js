import axios from 'axios';

const apiClient = axios.create({
    baseURL: 'http://localhost:5277/api',
});

// Interceptor to inject JWT Bearer token
apiClient.interceptors.request.use((config) => {
    const token = localStorage.getItem('bcl_token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
}, (error) => {
    return Promise.reject(error);
});

// Interceptor to handle unauthorized redirect
apiClient.interceptors.response.use((response) => response, (error) => {
    if (error.response && error.response.status === 401) {
        localStorage.removeItem('bcl_token');
        localStorage.removeItem('bcl_current_user');
        // Do not force redirect instantly on all page components if they have safe fallbacks, 
        // but standard POS should redirect to login if session expires
        window.location.href = '/login';
    }
    return Promise.reject(error);
});

export default apiClient;
