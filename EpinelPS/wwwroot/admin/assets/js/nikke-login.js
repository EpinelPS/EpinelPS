// NIKKE登录页专用脚本
document.addEventListener('DOMContentLoaded', function() {
    // 设置表单提交事件
    const form = document.getElementById('loginForm');
    if (form) {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            AdminLogin();
        });
    }
    
    // 允许按回车键提交
    const passwordInput = document.getElementById('PasswordBox');
    if (passwordInput) {
        passwordInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                AdminLogin();
            }
        });
    }
    
    // 检查是否已经登录
    const token = localStorage.getItem('token');
    if (token) {
        // 已登录用户直接跳转
        // window.location.pathname = "/admin/dashboard";
    }
    
    // 添加动画效果
    setTimeout(function() {
        const loginBox = document.querySelector('.login-box');
        if (loginBox) {
            loginBox.style.opacity = '1';
            loginBox.style.transform = 'translateY(0)';
        }
    }, 100);
});