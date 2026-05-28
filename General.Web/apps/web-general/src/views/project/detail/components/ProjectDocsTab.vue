<script lang="ts" setup>
import { Button, Card, Table, Tag } from 'ant-design-vue';

import type { ProjectApi } from '#/api/core';

import { useProjectDetailContext } from '../context';

const {
  detail,
  documentColumns,
  featuredDocuments,
  knowledgeCollections,
  loading,
  openDocumentWorkspace,
} = useProjectDetailContext();
</script>

<template>
<section class="project-detail__section">
  <div class="project-detail__knowledge-layout">
    <aside class="project-detail__knowledge-sidebar">
      <Card :bordered="false" class="project-detail__inner-card" title="知识库">
        <div class="project-detail__knowledge-groups">
          <article v-for="group in knowledgeCollections" :key="group.name" class="project-detail__knowledge-group">
            <div>
              <strong>{{ group.name }}</strong>
              <p>{{ group.description }}</p>
            </div>
            <span>{{ group.count }} 篇</span>
          </article>
        </div>
      </Card>
    </aside>
    <div class="project-detail__knowledge-main">
      <Card :bordered="false" class="project-detail__inner-card" title="知识文档">
        <div class="project-detail__knowledge-list">
          <article v-for="doc in featuredDocuments" :key="doc.id" class="project-detail__knowledge-doc">
            <div class="project-detail__knowledge-doc-main">
              <strong>{{ doc.title }}</strong>
              <div class="project-detail__knowledge-meta project-detail__knowledge-meta--single-line">
                <span>{{ doc.category || '未分类' }}</span>
                <span>{{ doc.version || 'v1.0' }}</span>
                <span>{{ doc.ownerName || '-' }}</span>
                <span>{{ doc.status || '-' }}</span>
              </div>
            </div>
            <Button type="link" @click="openDocumentWorkspace(doc)">新tab打开</Button>
          </article>
        </div>
      </Card>
      <Card :bordered="false" class="project-detail__inner-card" title="全部文档索引">
        <Table
          :columns="documentColumns"
          :data-source="detail?.documents || []"
          :loading="loading"
          :pagination="false"
          row-key="id"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'title'">
              <Button type="link" @click="openDocumentWorkspace(record as ProjectApi.DocumentItem)">
                {{ (record as ProjectApi.DocumentItem).title }}
              </Button>
            </template>
            <template v-else-if="column.key === 'status'">
              <Tag color="processing">{{ (record as ProjectApi.DocumentItem).status }}</Tag>
            </template>
          </template>
        </Table>
      </Card>
    </div>
  </div>
</section>
</template>
