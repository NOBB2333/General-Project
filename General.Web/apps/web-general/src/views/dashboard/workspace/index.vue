<script lang="ts" setup>
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import {
  Alert,
  Button,
  Card,
  Col,
  Progress,
  Row,
  Tag,
  Timeline,
} from 'ant-design-vue';

defineOptions({ name: 'GeneralWorkspaceDashboard' });

const router = useRouter();

const moduleCards = [
  {
    description: '需求、任务、缺陷、测试反馈统一挂接到一个项目视角。',
    progress: 30,
    title: '项目执行区',
  },
  {
    description: '合同、预算、回款、产值、资源投入逐步收敛到经营视图。',
    progress: 15,
    title: '经营管理区',
  },
  {
    description: '审计留痕、资料归档、权限隔离和 AI 预警规则后续逐步落地。',
    progress: 10,
    title: '治理与智能区',
  },
];

const todoItems = [
  '梳理一期核心流程与关键字段',
  '打通前端 app 与后端真实接口约定',
  '补齐项目、任务、会议纪要的原型页面',
  '确定桌面壳与 Web 的桥接边界',
];

const timelineItems = [
  { color: 'blue', content: '已完成 ABP 后端与 Avalonia 桌面壳初始化' },
  { color: 'green', content: '已创建 web-general 作为你自己的前端 app' },
  { color: 'orange', content: '下一步建议先做登录后首页与项目列表页' },
];

function gotoAnalytics() {
  router.push('/analytics');
}
</script>

<template>
  <Page
    description="当前页是 web-general 的专属首页占位，用来承接你后续自己的业务页面。"
    title="General 项目总览"
  >
    <Alert
      banner
      class="mb-4"
      message="当前先保留模板基础能力，但首页内容已经切换为你的项目方向。"
      type="info"
    />

    <Row :gutter="[16, 16]">
      <Col :lg="16" :span="24">
        <Card :bordered="false" title="一期建设聚焦">
          <div class="grid gap-4 md:grid-cols-3">
            <Card
              v-for="card in moduleCards"
              :key="card.title"
              :bordered="false"
              class="bg-[#f8fafc]"
            >
              <template #title>{{ card.title }}</template>
              <p class="mb-4 text-text-secondary">{{ card.description }}</p>
              <Progress :percent="card.progress" :show-info="false" />
              <div class="mt-3">
                <Tag color="blue">规划中</Tag>
              </div>
            </Card>
          </div>
        </Card>

        <Card :bordered="false" class="mt-4" title="当前建议动作">
          <Timeline>
            <Timeline.Item
              v-for="item in timelineItems"
              :key="item.content"
              :color="item.color"
            >
              {{ item.content }}
            </Timeline.Item>
          </Timeline>
        </Card>
      </Col>

      <Col :lg="8" :span="24">
        <Card :bordered="false" title="待办清单">
          <div class="flex flex-col gap-3">
            <div
              v-for="item in todoItems"
              :key="item"
              class="rounded-xl border border-solid border-[var(--ant-color-border-secondary)] px-4 py-3"
            >
              {{ item }}
            </div>
          </div>

          <div class="mt-4 flex gap-3">
            <Button type="primary" @click="gotoAnalytics">查看分析占位页</Button>
            <Button @click="router.push('/demos/ant-design')">保留模板演示页</Button>
          </div>
        </Card>
      </Col>
    </Row>
  </Page>
</template>
