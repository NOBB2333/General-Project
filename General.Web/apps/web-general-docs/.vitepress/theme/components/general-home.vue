<script setup lang="ts">
const metrics = [
  { label: '租户运维', value: 'Host' },
  { label: '权限模型', value: 'RBAC + Scope' },
  { label: '项目治理', value: 'PMO / PM' },
  { label: '部署形态', value: 'NUC / Cloud' },
];

const tracks = [
  {
    label: '平台治理',
    text: '租户、用户、角色、菜单、组织范围、配置、字典、缓存和开放接口统一治理。',
  },
  {
    label: '项目执行',
    text: '围绕项目列表、任务、里程碑、风险问题和成员协同组织工作流。',
  },
  {
    label: '经营分析',
    text: '把合同、预算、采购、回款和利润预警放到统一经营视角。',
  },
];

const layers = ['Vue 3', 'VitePress', 'ABP', '.NET', 'SQLite', 'PostgreSQL'];
</script>

<template>
  <main class="general-home">
    <section class="general-home__hero">
      <div class="general-home__field" aria-hidden="true">
        <span v-for="index in 18" :key="index" />
      </div>
      <div class="general-home__hero-inner">
        <div class="general-home__copy">
          <p class="general-home__eyebrow">Enterprise Operating System</p>
          <h1>General Admin</h1>
          <p class="general-home__lead">
            面向企业平台治理、项目执行和经营分析的一体化管理系统文档。
          </p>
          <div class="general-home__actions">
            <a class="general-home__button general-home__button--primary" href="/guide/introduction">
              阅读文档
            </a>
            <a class="general-home__button" href="/operations/deployment">
              部署指南
            </a>
          </div>
        </div>

        <div class="general-home__console" aria-label="General Admin capability map">
          <div class="general-home__console-top">
            <span />
            <span />
            <span />
          </div>
          <div class="general-home__console-grid">
            <div v-for="metric in metrics" :key="metric.label" class="general-home__metric">
              <span>{{ metric.label }}</span>
              <strong>{{ metric.value }}</strong>
            </div>
          </div>
          <div class="general-home__flow">
            <span>Tenant</span>
            <i />
            <span>Scope</span>
            <i />
            <span>Project</span>
            <i />
            <span>Audit</span>
          </div>
        </div>
      </div>
    </section>

    <section class="general-home__section general-home__section--tracks">
      <div class="general-home__section-head">
        <span>Documentation Tracks</span>
        <h2>平台能力、权限边界和运维动作按模块拆开说明。</h2>
      </div>
      <div class="general-home__tracks">
        <article v-for="track in tracks" :key="track.label" class="general-home__track">
          <span>{{ track.label }}</span>
          <p>{{ track.text }}</p>
        </article>
      </div>
    </section>

    <section class="general-home__section general-home__section--stack">
      <div class="general-home__stack-copy">
        <span>System Stack</span>
        <h2>后端、前端、迁移器和部署脚本按同一套发布链路交付。</h2>
        <p>
          文档站只改造 General Admin 自己的应用目录，保留 VitePress 的搜索、侧边栏和 Markdown 工作流，后续同步上游模板时边界清楚。
        </p>
      </div>
      <div class="general-home__layers">
        <span v-for="layer in layers" :key="layer">{{ layer }}</span>
      </div>
    </section>
  </main>
</template>
