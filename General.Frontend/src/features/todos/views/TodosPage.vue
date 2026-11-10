<script setup lang="ts">
import { todosModel } from '@/features/todos/model/todos.model';
import TodosPageSurface from './TodosPageSurface.vue';
</script>

<template>
  <TodosPageSurface :model="todosModel" />
</template>
