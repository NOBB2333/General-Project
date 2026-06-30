<script lang="ts" setup>
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue';

import { Graph } from '@antv/x6';

type WorkflowStep = {
  count: number;
  key: string;
  title: string;
};

type WorkflowPreviewGroup = {
  key: string;
  steps: WorkflowStep[];
  title: string;
  tone?: string;
};

const props = defineProps<{
  projectId?: string;
  workflows: WorkflowPreviewGroup[];
}>();

const containerRef = ref<HTMLDivElement | null>(null);
let graph: Graph | null = null;

const workflowKey = computed(() => props.workflows[0]?.key || 'default');

const storageKey = computed(() =>
  props.projectId
    ? `general:project-detail:${props.projectId}:${workflowKey.value}:workflow-canvas`
    : `general:project-detail:${workflowKey.value}:workflow-canvas`
);

function loadStoredPositions() {
  try {
    return JSON.parse(localStorage.getItem(storageKey.value) || '{}') as Record<
      string,
      { x: number; y: number }
    >;
  } catch {
    return {};
  }
}

function saveNodePositions() {
  if (!graph) {
    return;
  }

  const positions = graph
    .getNodes()
    .reduce<Record<string, { x: number; y: number }>>((result, node) => {
      result[node.id] = node.position();
      return result;
    }, {});
  localStorage.setItem(storageKey.value, JSON.stringify(positions));
}

function buildWorkflowCells() {
  const storedPositions = loadStoredPositions();
  const cells: Array<Record<string, unknown>> = [];
  const workflow = props.workflows[0];
  if (!workflow) {
    return cells;
  }

  workflow.steps.forEach((step, stepIndex) => {
    const id = `${workflow.key}-${step.key}`;
    const stored = storedPositions[id];
    const x = 36 + stepIndex * 188;
    const y = 36;
    cells.push({
      attrs: {
        body: {
          fill: '#ffffff',
          rx: 8,
          ry: 8,
          stroke:
            workflow.tone === 'danger'
              ? '#ff4d4f'
              : workflow.tone === 'warning'
              ? '#faad14'
              : '#1677ff',
          strokeWidth: 1,
        },
        count: {
          fill:
            workflow.tone === 'danger'
              ? '#fff1f0'
              : workflow.tone === 'warning'
              ? '#fffbe6'
              : '#e6f4ff',
          fontSize: 13,
          fontWeight: 700,
          refX: 14,
          refY: 14,
          text: `${step.count}`,
        },
        label: {
          fill: '#1f2937',
          fontSize: 13,
          fontWeight: 600,
          refX: 56,
          refY: 19,
          text: step.title,
          textAnchor: 'start',
        },
        title: {
          fill: '#6b7280',
          fontSize: 11,
          refX: 56,
          refY: 42,
          text: workflow.title,
          textAnchor: 'start',
        },
      },
      height: 72,
      id,
      markup: [
        { selector: 'body', tagName: 'rect' },
        { selector: 'count', tagName: 'text' },
        { selector: 'label', tagName: 'text' },
        { selector: 'title', tagName: 'text' },
      ],
      shape: 'rect',
      width: 152,
      x: stored?.x ?? x,
      y: stored?.y ?? y,
    });

    if (stepIndex > 0) {
      cells.push({
        attrs: {
          line: {
            stroke: '#94a3b8',
            strokeWidth: 1.5,
            targetMarker: {
              name: 'block',
              size: 6,
            },
          },
        },
        id: `${workflow.key}-${workflow.steps[stepIndex - 1]?.key}-${step.key}`,
        shape: 'edge',
        source: { cell: `${workflow.key}-${workflow.steps[stepIndex - 1]?.key}` },
        target: { cell: id },
      });
    }
  });

  return cells;
}

async function renderCanvas() {
  await nextTick();
  const container = containerRef.value;
  if (!container) {
    return;
  }

  graph?.dispose();
  graph = new Graph({
    async: false,
    background: {
      color: '#f8fafc',
    },
    connecting: {
      allowBlank: false,
      allowLoop: false,
      allowNode: true,
    },
    container,
    grid: {
      args: {
        color: '#e5e7eb',
        thickness: 1,
      },
      size: 12,
      visible: true,
    },
    height: 260,
    interacting: {
      edgeMovable: false,
      magnetConnectable: false,
    },
    panning: true,
    width: Math.max(container.clientWidth, 760),
  });

  graph.fromJSON(buildWorkflowCells());
  graph.on('node:moved', saveNodePositions);
}

onMounted(renderCanvas);
watch(
  () => [props.projectId, props.workflows],
  () => {
    void renderCanvas();
  },
  { deep: true }
);
onBeforeUnmount(() => {
  graph?.dispose();
  graph = null;
});
</script>

<template>
  <div class="project-detail__x6-canvas" ref="containerRef" />
</template>
