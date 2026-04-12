<script lang="ts" setup>
import { computed, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Alert,
  Button,
  Card,
  Col,
  Progress,
  Row,
  Segmented,
  Statistic,
  Table,
  Tag,
} from 'ant-design-vue';

defineOptions({ name: 'GeneralAnalyticsDashboard' });

const mode = ref<'project' | 'operation'>('project');

const riskRows = [
  {
    key: '1',
    level: '高',
    name: '需求范围持续膨胀',
    owner: '产品负责人',
    status: '待处理',
  },
  {
    key: '2',
    level: '中',
    name: '合同资料未完成归档',
    owner: '项目经理',
    status: '跟进中',
  },
  {
    key: '3',
    level: '中',
    name: '测试反馈闭环不完整',
    owner: '测试负责人',
    status: '待确认',
  },
];

const riskColumns = [
  { dataIndex: 'name', key: 'name', title: '事项' },
  { dataIndex: 'level', key: 'level', title: '等级' },
  { dataIndex: 'owner', key: 'owner', title: '负责人' },
  { dataIndex: 'status', key: 'status', title: '状态' },
];

const summary = computed(() => {
  if (mode.value === 'project') {
    return {
      archive: 42,
      compliance: 68,
      healthy: 76,
      warning: 3,
    };
  }

  return {
    archive: 55,
    compliance: 72,
    healthy: 81,
    warning: 2,
  };
});
</script>

<template>
  <Page
    description="这里先放一个经营分析与预警占位页，后续可逐步接入真实指标、ECharts 和 AI 预警。"
    title="经营分析占位页"
  >
    <div class="mb-4 flex items-center justify-between gap-3">
      <Alert
        class="flex-1"
        message="当前数据为占位演示数据，用来承接你后续的经营分析、审计预警和资源产能看板。"
        type="warning"
      />
      <Segmented
        v-model:value="mode"
        :options="[
          { label: '项目视角', value: 'project' },
          { label: '经营视角', value: 'operation' },
        ]"
      />
    </div>

    <Row :gutter="[16, 16]">
      <Col :lg="6" :md="12" :span="24">
        <Card :bordered="false">
          <Statistic title="健康度" :value="summary.healthy" suffix="%" />
        </Card>
      </Col>
      <Col :lg="6" :md="12" :span="24">
        <Card :bordered="false">
          <Statistic title="预警数量" :value="summary.warning" />
        </Card>
      </Col>
      <Col :lg="6" :md="12" :span="24">
        <Card :bordered="false">
          <Statistic title="归档完成数" :value="summary.archive" />
        </Card>
      </Col>
      <Col :lg="6" :md="12" :span="24">
        <Card :bordered="false">
          <Statistic title="合规评分" :value="summary.compliance" suffix="分" />
        </Card>
      </Col>
    </Row>

    <Row :gutter="[16, 16]" class="mt-1">
      <Col :lg="10" :span="24">
        <Card :bordered="false" title="关键能力准备度">
          <div class="flex flex-col gap-5">
            <div>
              <div class="mb-2 flex items-center justify-between">
                <span>项目主数据</span>
                <Tag color="blue">一期</Tag>
              </div>
              <Progress :percent="35" />
            </div>
            <div>
              <div class="mb-2 flex items-center justify-between">
                <span>资料归档与审计</span>
                <Tag color="gold">一期</Tag>
              </div>
              <Progress :percent="28" status="active" />
            </div>
            <div>
              <div class="mb-2 flex items-center justify-between">
                <span>经营分析下钻</span>
                <Tag color="purple">二期</Tag>
              </div>
              <Progress :percent="12" />
            </div>
            <div>
              <div class="mb-2 flex items-center justify-between">
                <span>AI 预警与经营分析</span>
                <Tag color="cyan">三期</Tag>
              </div>
              <Progress :percent="8" />
            </div>
          </div>
        </Card>
      </Col>

      <Col :lg="14" :span="24">
        <Card :bordered="false" title="当前重点风险">
          <Table
            :columns="riskColumns"
            :data-source="riskRows"
            :pagination="false"
            row-key="key"
            size="middle"
          />

          <div class="mt-4">
            <Button type="primary">后续接入真实预警规则</Button>
          </div>
        </Card>
      </Col>
    </Row>
  </Page>
</template>
