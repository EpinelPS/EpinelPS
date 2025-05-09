/**
 * NIKKE控制台国际化支持
 * 支持多语言切换、本地化和文本资源管理
 * 已优化性能，防止无限循环
 */
class NikkeI18n {
  constructor() {
    // 支持的语言
    this.supportedLanguages = ['zh', 'en', 'ja', 'ko'];
    
    // 语言名称映射
    this.languageNames = {
      'zh': '简体中文',
      'en': 'English',
      'ja': '日本語',
      'ko': '한국어'
    };
    
    // 当前语言
    this.currentLang = localStorage.getItem('nikke_lang') || this.detectBrowserLanguage() || 'zh';
    
    // 资源缓存
    this.resources = {};
    
    // 添加状态锁，防止循环调用
    this.isApplyingLanguage = false;
    this.isRenderingSwitcher = false;
    
    // 加载失败计数器
    this.loadFailureCount = 0;
    this.maxFailureRetries = 2;
    
    // MutationObserver实例
    this.observer = null;
    
    // 初始化
    this.init();
  }
  
  /**
   * 初始化国际化系统
   */
  async init() {
    try {
      // 设置加载状态
      document.documentElement.classList.add('lang-loading');
      
      // 加载当前语言资源，添加超时处理
      await Promise.race([
        this.loadLanguage(this.currentLang),
        new Promise((_, reject) => setTimeout(() => reject(new Error('Language loading timeout')), 5000))
      ]);
      
      // 应用语言
      this.applyLanguage();
      
      // 注册事件监听器（优化版本）
      this.registerEventListeners();
      
      // 显示语言切换器，延迟加载减少初始化负担
      setTimeout(() => this.renderLanguageSwitcher(), 1000);
      
      // 触发初始化完成事件
      window.dispatchEvent(new CustomEvent('nikke:i18n-ready'));
      
      console.log(`国际化系统初始化完成，当前语言: ${this.currentLang}`);
    } catch (error) {
      console.error('国际化系统初始化失败:', error);
      // 失败后应用默认语言，防止UI卡死
      this.fallbackToDefaultLanguage();
    } finally {
      // 无论成功失败，都移除加载状态
      document.documentElement.classList.remove('lang-loading');
    }
  }
  
  /**
   * 检测浏览器语言
   * @returns {string} 语言代码
   */
  detectBrowserLanguage() {
    try {
      const browserLang = navigator.language || navigator.userLanguage;
      if (!browserLang) return 'en';
      
      const baseLang = browserLang.split('-')[0]; // 提取基础语言代码 (zh-CN -> zh)
      
      if (this.supportedLanguages.includes(baseLang)) {
        return baseLang;
      }
      
      return 'en'; // 默认返回英语
    } catch (error) {
      console.error('语言检测失败:', error);
      return 'en';
    }
  }
  
  /**
   * 添加失败回退机制
   * 创建最小化语言资源，确保UI不会卡住
   */
  fallbackToDefaultLanguage() {
    this.currentLang = 'en';
    
    // 创建一个最小的语言资源对象，确保UI不会卡住
    this.resources[this.currentLang] = this.resources[this.currentLang] || {
      app: { 
        name: "NIKKE Admin Console", 
        version: "v2.5.3",
        footer: "© 2025 NIKKE Admin Console"
      },
      auth: { 
        login: "Login", 
        username: "Username", 
        password: "Password",
        enter: "Enter Console",
        welcome: "Please login to access admin features",
        verifying: "Verifying...",
        success: "Success",
        error: {
          required: "Username and password required",
          invalid: "Invalid credentials",
          network: "Network error"
        }
      },
      common: {
        loading: "Loading...",
        confirm: "Confirm",
        cancel: "Cancel"
      }
    };
    
    // 尝试设置当前语言存储
    try {
      localStorage.setItem('nikke_lang', this.currentLang);
    } catch (e) {
      console.warn('无法设置语言首选项:', e);
    }
    
    // 尝试应用这个最小语言资源
    this.applyLanguage();
    
    // 创建提示消息
    this.showFallbackMessage();
  }
  
  /**
   * 显示回退提示
   */
  showFallbackMessage() {
    try {
      const message = document.createElement('div');
      message.style.cssText = 'position:fixed;top:10px;right:10px;background:rgba(244,66,131,0.9);color:white;padding:10px 15px;border-radius:4px;z-index:9999;font-size:14px;max-width:300px;box-shadow:0 4px 12px rgba(0,0,0,0.2);';
      message.innerHTML = `
        <div style="display:flex;align-items:center;margin-bottom:8px;">
          <i class="fas fa-exclamation-triangle" style="margin-right:8px;"></i>
          <strong>Language Error</strong>
        </div>
        <p style="margin:0;line-height:1.4;">Language resources failed to load. Using fallback English.</p>
      `;
      
      document.body.appendChild(message);
      
      // 8秒后自动移除
      setTimeout(() => {
        message.style.opacity = '0';
        message.style.transition = 'opacity 0.5s ease';
        setTimeout(() => message.remove(), 500);
      }, 8000);
    } catch (e) {
      console.warn('无法显示回退消息:', e);
    }
  }
  
  /**
   * 加载语言资源
   * @param {string} lang - 语言代码
   * @returns {Promise<void>}
   */
  async loadLanguage(lang) {
    // 如果已经缓存，直接返回
    if (this.resources[lang]) {
      return;
    }
    
    // 如果已经超过最大重试次数，直接回退
    if (this.loadFailureCount >= this.maxFailureRetries) {
      throw new Error(`超过最大加载重试次数: ${this.maxFailureRetries}`);
    }
    
    try {
      // 添加AbortController支持超时取消
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), 3000);
      
      const response = await fetch(`/admin/assets/i18n/${lang}.json`, {
        signal: controller.signal,
        headers: {
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache'
        }
      });
      
      clearTimeout(timeoutId);
      
      if (!response.ok) {
        throw new Error(`Failed to load language: ${lang}, status: ${response.status}`);
      }
      
      const data = await response.json();
      
      // 验证资源格式
      if (!this.validateResource(data)) {
        throw new Error(`Invalid language resource format: ${lang}`);
      }
      
      this.resources[lang] = data;
      this.loadFailureCount = 0; // 重置失败计数
      console.log(`语言资源加载成功: ${lang}`);
    } catch (error) {
      console.error(`加载语言失败: ${lang}`, error);
      this.loadFailureCount++;
      
      // 如果加载失败且不是英语，尝试回退到英语
      if (lang !== 'en') {
        try {
          await this.loadLanguage('en');
          this.currentLang = 'en';
        } catch (fallbackError) {
          // 如果英语也加载失败，创建最小资源对象
          this.fallbackToDefaultLanguage();
        }
      } else {
        // 如果是英语加载失败，创建最小资源对象
        this.fallbackToDefaultLanguage();
      }
    }
  }
  
  /**
   * 验证资源格式
   * @param {Object} resource - 语言资源
   * @returns {boolean} 是否有效
   */
  validateResource(resource) {
    // 基本验证
    if (!resource || typeof resource !== 'object') {
      return false;
    }
    
    // 检查必要的顶级键
    const requiredKeys = ['app', 'auth'];
    for (const key of requiredKeys) {
      if (!resource[key] || typeof resource[key] !== 'object') {
        return false;
      }
    }
    
    return true;
  }
  
  /**
   * 切换语言
   * @param {string} lang - 语言代码
   */
  async switchLanguage(lang) {
    if (!this.supportedLanguages.includes(lang)) {
      console.error(`不支持的语言: ${lang}`);
      return;
    }
    
    try {
      // 设置加载状态
      document.documentElement.classList.add('lang-loading');
      
      // 加载所选语言
      await this.loadLanguage(lang);
      
      // 保存当前语言
      this.currentLang = lang;
      localStorage.setItem('nikke_lang', lang);
      
      // 应用语言
      this.applyLanguage();
      
      // 更新语言切换器
      this.updateLanguageSwitcher();
      
      // 触发语言变更事件
      window.dispatchEvent(new CustomEvent('nikke:languageChanged', { detail: { lang } }));
      
      console.log(`语言已切换: ${lang}`);
    } catch (error) {
      console.error(`语言切换失败: ${lang}`, error);
    } finally {
      // 移除加载状态
      document.documentElement.classList.remove('lang-loading');
    }
  }
  
  /**
   * 根据键获取翻译文本
   * @param {string} key - 点符号分隔的键 (例如 "auth.login")
   * @param {Object} [params] - 替换参数
   * @returns {string} 翻译后的文本
   */
  t(key, params = {}) {
    try {
      // 确保资源已加载
      if (!this.resources[this.currentLang]) {
        return key;
      }
      
      // 解析键
      const keys = key.split('.');
      let value = this.resources[this.currentLang];
      
      // 遍历键获取值
      for (const k of keys) {
        value = value?.[k];
        if (value === undefined) {
          // 尝试从英语资源中找
          if (this.resources['en']) {
            let enValue = this.resources['en'];
            for (const k of keys) {
              enValue = enValue?.[k];
              if (enValue === undefined) {
                return key;
              }
            }
            return typeof enValue === 'string' ? this.replaceParams(enValue, params) : key;
          }
          // 如果找不到值，返回键
          return key;
        }
      }
      
      // 如果不是字符串，返回键
      if (typeof value !== 'string') {
        return key;
      }
      
      // 替换参数
      return this.replaceParams(value, params);
    } catch (error) {
      console.error(`翻译键失败: ${key}`, error);
      return key;
    }
  }
  
  /**
   * 替换字符串中的参数
   * @param {string} text - 原始文本
   * @param {Object} params - 替换参数
   * @returns {string} 替换后的文本
   */
  replaceParams(text, params) {
    if (!text) return '';
    return text.replace(/\{\{([^}]+)\}\}/g, (_, key) => {
      return params[key.trim()] !== undefined ? params[key.trim()] : `{{${key}}}`;
    });
  }
  
  /**
   * 应用语言到页面
   */
  applyLanguage() {
    // 防止循环调用
    if (this.isApplyingLanguage) return;
    this.isApplyingLanguage = true;
    
    try {
      // 临时关闭MutationObserver
      if (this.observer) {
        this.observer.disconnect();
      }
      
      // 应用到所有带有 data-i18n 属性的元素
      document.querySelectorAll('[data-i18n]').forEach(element => {
        const key = element.getAttribute('data-i18n');
        const oldText = element.textContent;
        const newText = this.t(key);
        if (oldText !== newText) {
          element.textContent = newText;
        }
      });
      
      // 应用到所有带有 data-i18n-placeholder 属性的元素
      document.querySelectorAll('[data-i18n-placeholder]').forEach(element => {
        const key = element.getAttribute('data-i18n-placeholder');
        const oldPlaceholder = element.placeholder;
        const newPlaceholder = this.t(key);
        if (oldPlaceholder !== newPlaceholder) {
          element.placeholder = newPlaceholder;
        }
      });
      
      // 应用到所有带有 data-i18n-title 属性的元素
      document.querySelectorAll('[data-i18n-title]').forEach(element => {
        const key = element.getAttribute('data-i18n-title');
        const oldTitle = element.title;
        const newTitle = this.t(key);
        if (oldTitle !== newTitle) {
          element.title = newTitle;
        }
      });
      
      // 设置HTML和文档语言属性
      document.documentElement.lang = this.currentLang;
    } catch (error) {
      console.error('应用语言失败:', error);
    } finally {
      // 重新启动MutationObserver
      if (this.observer) {
        const mainContent = document.querySelector('.login-container') || document.body;
        this.observer.observe(mainContent, {
          childList: true,
          subtree: true
        });
      }
      
      // 确保锁始终被释放
      setTimeout(() => {
        this.isApplyingLanguage = false;
      }, 50);
    }
  }
  
  /**
   * 注册事件监听器
   */
  registerEventListeners() {
    // 清理旧的观察器
    if (this.observer) {
      this.observer.disconnect();
      this.observer = null;
    }
    
    // 防抖函数
    const debounce = (func, wait) => {
      let timeout;
      return function(...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
      };
    };
    
    // 优化MutationObserver，只在需要时触发
    const debouncedApplyLanguage = debounce(() => {
      if (!this.isApplyingLanguage) {
        this.applyLanguage();
      }
    }, 200);
    
    // 创建新的观察器
    this.observer = new MutationObserver(mutations => {
      // 如果当前正在应用语言或渲染切换器，忽略这些变化
      if (this.isApplyingLanguage || this.isRenderingSwitcher) {
        return;
      }
      
      // 检查是否有需要处理的变化
      const hasRelevantChanges = mutations.some(mutation => {
        if (mutation.type !== 'childList' || !mutation.addedNodes.length) {
          return false;
        }
        
        // 检查添加的节点是否包含需要国际化的元素
        return Array.from(mutation.addedNodes).some(node => {
          // 忽略文本节点和注释节点
          if (node.nodeType !== Node.ELEMENT_NODE) {
            return false;
          }
          
          // 忽略语言切换器相关的元素，避免无限循环
          if (node.classList && (
            node.classList.contains('language-switcher') || 
            node.id === 'nikke-i18n-styles'
          )) {
            return false;
          }
          
          // 检查节点本身或其子节点是否有国际化属性
          return (
            (node.hasAttribute && (
              node.hasAttribute('data-i18n') || 
              node.hasAttribute('data-i18n-placeholder') || 
              node.hasAttribute('data-i18n-title')
            )) ||
            (node.querySelector && node.querySelector('[data-i18n], [data-i18n-placeholder], [data-i18n-title]'))
          );
        });
      });
      
      if (hasRelevantChanges) {
        debouncedApplyLanguage();
      }
    });
    
    // 改为只观察主要内容区域，而不是整个body
    const mainContent = document.querySelector('.login-container, .nikke-dashboard') || document.body;
    this.observer.observe(mainContent, {
      childList: true,
      subtree: true
    });
    
    // 页面加载完成后应用语言
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', () => {
        this.applyLanguage();
      });
    } else {
      // 如果页面已加载完成，直接应用
      this.applyLanguage();
    }
    
    // 添加可见性变化事件监听
    document.addEventListener('visibilitychange', () => {
      if (!document.hidden) {
        // 页面重新变为可见时，刷新语言
        this.applyLanguage();
      }
    });
  }
  
  /**
 * 渲染语言切换器
 */
renderLanguageSwitcher() {
  // 防止重复渲染
  if (this.isRenderingSwitcher) return;
  this.isRenderingSwitcher = true;
  
  try {
    // 检查是否已存在语言切换器
    if (document.querySelector('.language-switcher')) {
      this.isRenderingSwitcher = false;
      return;
    }
    
    // 临时关闭MutationObserver
    if (this.observer) {
      this.observer.disconnect();
    }
    
    // 创建语言切换器容器
    const switcherContainer = document.createElement('div');
    switcherContainer.className = 'language-switcher';
    switcherContainer.innerHTML = `
      <div class="language-switcher-toggle">
        <i class="fas fa-globe"></i>
        <span class="current-language">${this.languageNames[this.currentLang]}</span>
        <i class="fas fa-chevron-down"></i>
      </div>
      <div class="language-options"></div>
    `;
    
    // 创建语言选项
    const languageOptions = switcherContainer.querySelector('.language-options');
    this.supportedLanguages.forEach(lang => {
      const option = document.createElement('div');
      option.className = `language-option ${lang === this.currentLang ? 'active' : ''}`;
      option.setAttribute('data-lang', lang);
      option.innerHTML = `
        <span class="language-name">${this.languageNames[lang]}</span>
        <span class="language-code">${lang.toUpperCase()}</span>
      `;
      
      option.addEventListener('click', (e) => {
        e.stopPropagation();
        this.switchLanguage(lang);
        switcherContainer.classList.remove('open');
      });
      languageOptions.appendChild(option);
    });
    
    // 切换显示语言下拉菜单
    const toggleButton = switcherContainer.querySelector('.language-switcher-toggle');
    toggleButton.addEventListener('click', (e) => {
      e.stopPropagation();
      switcherContainer.classList.toggle('open');
    });
    
    // 点击外部关闭下拉菜单
    const clickOutside = (e) => {
      if (!switcherContainer.contains(e.target)) {
        switcherContainer.classList.remove('open');
      }
    };
    
    document.addEventListener('click', clickOutside);
    
    // 为切换器添加销毁方法
    switcherContainer._destroy = () => {
      document.removeEventListener('click', clickOutside);
      switcherContainer.remove();
    };
    
    // 检查是否有导航栏语言切换器插槽
    const navbarSwitcherSlot = document.getElementById('navbar-language-switcher');
    
    // 如果是登录页面，或者没有找到导航栏插槽
    if (document.querySelector('body.login-page') || !navbarSwitcherSlot) {
      // 添加到body，使用固定定位
      document.body.appendChild(switcherContainer);
    } else {
      // 如果找到导航栏插槽，放入插槽
      navbarSwitcherSlot.appendChild(switcherContainer);
      // 添加导航栏集成样式类
      switcherContainer.classList.add('navbar-integrated');
    }
  } catch (error) {
    console.error('渲染语言切换器失败:', error);
  } finally {
    // 重新启动MutationObserver
    if (this.observer) {
      const mainContent = document.querySelector('.login-container') || document.body;
      this.observer.observe(mainContent, {
        childList: true,
        subtree: true
      });
    }
    
    // 确保锁始终被释放
    setTimeout(() => {
      this.isRenderingSwitcher = false;
    }, 50);
  }
}
  
  /**
   * 更新语言切换器
   */
  updateLanguageSwitcher() {
    try {
      const currentLanguageElement = document.querySelector('.language-switcher .current-language');
      if (currentLanguageElement) {
        currentLanguageElement.textContent = this.languageNames[this.currentLang];
      }
      
      const options = document.querySelectorAll('.language-switcher .language-option');
      options.forEach(option => {
        const lang = option.getAttribute('data-lang');
        if (lang === this.currentLang) {
          option.classList.add('active');
        } else {
          option.classList.remove('active');
        }
      });
    } catch (error) {
      console.error('更新语言切换器失败:', error);
    }
  }
  
  /**
   * 销毁国际化实例
   */
  destroy() {
    try {
      // 断开观察器
      if (this.observer) {
        this.observer.disconnect();
        this.observer = null;
      }
      
      // 移除语言切换器
      const switcher = document.querySelector('.language-switcher');
      if (switcher && switcher._destroy) {
        switcher._destroy();
      } else if (switcher) {
        switcher.remove();
      }
      
      // 清理事件
      window.removeEventListener('nikke:languageChanged', this._languageChangedHandler);
      
      console.log('国际化系统已销毁');
    } catch (error) {
      console.error('销毁国际化系统时出错:', error);
    }
  }
}

// 使用防抖动初始化，避免多次初始化
window.i18n = (() => {
  let instance = null;
  return () => {
    if (!instance) {
      instance = new NikkeI18n();
    }
    return instance;
  };
})()();

// 添加安全超时，防止页面卡死
const safetyTimeout = setTimeout(() => {
  try {
    console.warn('国际化系统安全检查...');
    
    // 检查页面是否已经渲染完成
    const loginBox = document.querySelector('.login-box');
    
    // 如果页面关键元素没有正确渲染或页面显示加载状态超过8秒
    if (document.documentElement.classList.contains('lang-loading') || 
        (loginBox && getComputedStyle(loginBox).opacity === '0')) {
      console.error('检测到页面可能已卡死，尝试恢复...');
      
      // 移除加载状态
      document.documentElement.classList.remove('lang-loading');
      
      // 尝试重置国际化系统
      if (window.i18n) {
        // 临时停用观察器
        if (window.i18n.observer) {
          window.i18n.observer.disconnect();
        }
        window.i18n.isApplyingLanguage = false;
        window.i18n.isRenderingSwitcher = false;
      }
      
      // 强制显示登录框
      if (loginBox) {
        loginBox.style.opacity = '1';
        loginBox.style.transform = 'translateY(0)';
      }
      
      // 显示恢复提示
      const recoveryMessage = document.createElement('div');
      recoveryMessage.style.cssText = 'position:fixed;bottom:10px;left:10px;background:rgba(0,0,0,0.7);color:white;padding:10px;border-radius:4px;z-index:9999;font-size:12px;';
      recoveryMessage.textContent = '页面已恢复，但某些功能可能不可用。如需完整体验，请刷新页面。';
      document.body.appendChild(recoveryMessage);
      
      // 10秒后自动移除提示
      setTimeout(() => recoveryMessage.remove(), 10000);
    }
  } catch (e) {
    console.error('安全检查失败', e);
  }
}, 8000); // 8秒后检查

// 如果页面正常加载完成，清除安全超时
window.addEventListener('load', () => {
  clearTimeout(safetyTimeout);
  console.log('页面加载完成，取消安全检查');
});