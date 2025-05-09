/**
 * NIKKE控制台国际化支持 - 同步版
 */
class NikkeI18n {
  constructor() {
    this.supportedLanguages = ['zh', 'en', 'ja', 'ko'];
    this.languageNames = {'zh': '简体中文', 'en': 'English', 'ja': '日本語', 'ko': '한국어'};
    this.currentLang = localStorage.getItem('nikke_lang') || this.detectBrowserLanguage() || 'zh';
    this.resources = {
      // 默认内置基础英文翻译，防止加载失败
      'en': {
        app: {name: "NIKKE Admin Console", version: "v2.5.3"},
        auth: {login: "Login", username: "Username", password: "Password"},
        common: {loading: "Loading...", confirm: "Confirm", cancel: "Cancel"}
      }
    };
    this.isApplyingLanguage = false;
    this.isRenderingSwitcher = false;
    this.observer = null;
    
    // 立即同步加载所有语言文件
    this.loadAllLanguages();
    this.init();
  }
  
  init() {
    try {
      document.documentElement.classList.add('lang-loading');
      this.applyLanguage();
      this.registerEventListeners();
      this.renderLanguageSwitcher();
      window.dispatchEvent(new CustomEvent('nikke:i18n-ready'));
    } catch (error) {
      console.error('初始化失败:', error);
      this.fallbackToDefaultLanguage();
    } finally {
      document.documentElement.classList.remove('lang-loading');
    }
  }
  
  detectBrowserLanguage() {
    try {
      const browserLang = navigator.language || navigator.userLanguage;
      if (!browserLang) return 'en';
      const baseLang = browserLang.split('-')[0];
      return this.supportedLanguages.includes(baseLang) ? baseLang : 'en';
    } catch {
      return 'en';
    }
  }
  
  fallbackToDefaultLanguage() {
    this.currentLang = 'en';
    try {
      localStorage.setItem('nikke_lang', this.currentLang);
    } catch (e) {}
    this.applyLanguage();
  }
  
  loadAllLanguages() {
    // 同步预加载所有语言文件
    for (const lang of this.supportedLanguages) {
      if (this.resources[lang]) continue;
      
      try {
        // 使用同步XMLHttpRequest加载语言文件
        const xhr = new XMLHttpRequest();
        xhr.open('GET', `/admin/assets/i18n/${lang}.json`, false); // false表示同步请求
        xhr.send();
        
        if (xhr.status === 200) {
          const data = JSON.parse(xhr.responseText);
          if (data && typeof data === 'object') {
            this.resources[lang] = data;
          } else {
            throw new Error('格式无效');
          }
        } else {
          throw new Error(`加载失败: ${lang} (${xhr.status})`);
        }
      } catch (error) {
        console.error(`加载语言文件失败: ${lang}`, error);
        if (lang !== 'en') {
          // 英文是基础语言，已有内置默认值
          continue;
        }
      }
    }
    
    // 确保当前语言已加载
    if (!this.resources[this.currentLang]) {
      // 当前语言未加载成功，回退到英文
      console.warn(`当前语言 ${this.currentLang} 加载失败，回退到英文`);
      this.currentLang = 'en';
      try {
        localStorage.setItem('nikke_lang', 'en');
      } catch (e) {}
    }
  }
  
  switchLanguage(lang) {
    if (!this.supportedLanguages.includes(lang)) return;
    
    try {
      document.documentElement.classList.add('lang-loading');
      
      // 确保语言资源已加载
      if (!this.resources[lang]) {
        try {
          // 尝试同步加载缺失的语言资源
          const xhr = new XMLHttpRequest();
          xhr.open('GET', `/admin/assets/i18n/${lang}.json`, false);
          xhr.send();
          
          if (xhr.status === 200) {
            const data = JSON.parse(xhr.responseText);
            if (data && typeof data === 'object') {
              this.resources[lang] = data;
            } else {
              throw new Error('格式无效');
            }
          } else {
            throw new Error(`加载失败: ${lang} (${xhr.status})`);
          }
        } catch (error) {
          console.error(`切换到语言 ${lang} 时加载失败`, error);
          return;
        }
      }
      
      this.currentLang = lang;
      localStorage.setItem('nikke_lang', lang);
      this.applyLanguage();
      this.updateLanguageSwitcher();
      window.dispatchEvent(new CustomEvent('nikke:languageChanged', {detail: {lang}}));
    } catch (error) {
      console.error(`切换失败: ${lang}`, error);
    } finally {
      document.documentElement.classList.remove('lang-loading');
    }
  }
  
  t(key, params = {}) {
    try {
      if (!this.resources[this.currentLang]) return key;
      
      const keys = key.split('.');
      let value = this.resources[this.currentLang];
      
      for (const k of keys) {
        value = value?.[k];
        if (value === undefined) {
          if (this.resources['en']) {
            let enValue = this.resources['en'];
            for (const k of keys) {
              enValue = enValue?.[k];
              if (enValue === undefined) return key;
            }
            return typeof enValue === 'string' ? this.replaceParams(enValue, params) : key;
          }
          return key;
        }
      }
      
      return typeof value === 'string' ? this.replaceParams(value, params) : key;
    } catch {
      return key;
    }
  }
  
  replaceParams(text, params) {
    if (!text) return '';
    return text.replace(/\{\{([^}]+)\}\}/g, (_, key) => 
      params[key.trim()] !== undefined ? params[key.trim()] : `{{${key}}}`
    );
  }
  
  applyLanguage() {
    if (this.isApplyingLanguage) return;
    this.isApplyingLanguage = true;
    
    try {
      if (this.observer) this.observer.disconnect();
      
      document.querySelectorAll('[data-i18n]').forEach(element => {
        const key = element.getAttribute('data-i18n');
        element.textContent = this.t(key);
      });
      
      document.querySelectorAll('[data-i18n-placeholder]').forEach(element => {
        const key = element.getAttribute('data-i18n-placeholder');
        element.placeholder = this.t(key);
      });
      
      document.querySelectorAll('[data-i18n-title]').forEach(element => {
        const key = element.getAttribute('data-i18n-title');
        element.title = this.t(key);
      });
      
      document.documentElement.lang = this.currentLang;
    } catch (error) {
      console.error('应用语言失败:', error);
    } finally {
      if (this.observer) {
        const mainContent = document.querySelector('.login-container') || document.body;
        this.observer.observe(mainContent, {childList: true, subtree: true});
      }
      this.isApplyingLanguage = false;
    }
  }
  
  registerEventListeners() {
    if (this.observer) {
      this.observer.disconnect();
      this.observer = null;
    }
    
    this.observer = new MutationObserver(mutations => {
      if (this.isApplyingLanguage || this.isRenderingSwitcher) return;
      
      const hasRelevantChanges = mutations.some(mutation => {
        if (mutation.type !== 'childList' || !mutation.addedNodes.length) return false;
        
        return Array.from(mutation.addedNodes).some(node => {
          if (node.nodeType !== Node.ELEMENT_NODE) return false;
          
          if (node.classList && (
            node.classList.contains('language-switcher') || 
            node.id === 'nikke-i18n-styles'
          )) return false;
          
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
      
      if (hasRelevantChanges && !this.isApplyingLanguage) {
        this.applyLanguage();
      }
    });
    
    const mainContent = document.querySelector('.login-container, .nikke-dashboard') || document.body;
    this.observer.observe(mainContent, {childList: true, subtree: true});
    
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', () => this.applyLanguage());
    } else {
      this.applyLanguage();
    }
    
    document.addEventListener('visibilitychange', () => {
      if (!document.hidden) this.applyLanguage();
    });
  }
  
  renderLanguageSwitcher() {
    if (this.isRenderingSwitcher) return;
    this.isRenderingSwitcher = true;
    
    try {
      if (document.querySelector('.language-switcher')) {
        this.isRenderingSwitcher = false;
        return;
      }
      
      if (this.observer) this.observer.disconnect();
      
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
      
      const toggle = switcherContainer.querySelector('.language-switcher-toggle');
      toggle.addEventListener('click', (e) => {
        e.stopPropagation();
        switcherContainer.classList.toggle('open');
      });
      
      // 点击外部区域关闭语言选择器
      const clickOutside = (e) => {
        if (!switcherContainer.contains(e.target)) {
          switcherContainer.classList.remove('open');
        }
      };
      
      document.addEventListener('click', clickOutside);
      
      // 找到合适的插入位置
      let target = document.getElementById('navbar-language-switcher');
      if (target) {
        target.appendChild(switcherContainer);
        switcherContainer.classList.add('navbar-integrated');
      } else {
        // 后备插入到body
        document.body.appendChild(switcherContainer);
      }
      
      // 恢复DOM观察
      if (this.observer) {
        const mainContent = document.querySelector('.login-container') || document.body;
        this.observer.observe(mainContent, {childList: true, subtree: true});
      }
    } catch (error) {
      console.error('渲染语言切换器失败:', error);
    } finally {
      this.isRenderingSwitcher = false;
    }
  }
  
  updateLanguageSwitcher() {
    try {
      const switcher = document.querySelector('.language-switcher');
      if (!switcher) return;
      
      const currentLanguageSpan = switcher.querySelector('.current-language');
      if (currentLanguageSpan) {
        currentLanguageSpan.textContent = this.languageNames[this.currentLang];
      }
      
      const options = switcher.querySelectorAll('.language-option');
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
  
  destroy() {
    if (this.observer) {
      this.observer.disconnect();
      this.observer = null;
    }
    
    // 移除事件监听
    document.removeEventListener('DOMContentLoaded', this.applyLanguage);
    document.removeEventListener('visibilitychange', () => {
      if (!document.hidden) this.applyLanguage();
    });
    
    // 移除语言切换器
    const switcher = document.querySelector('.language-switcher');
    if (switcher) {
      switcher.remove();
    }
  }
}

// 实例化
window.i18n = new NikkeI18n();