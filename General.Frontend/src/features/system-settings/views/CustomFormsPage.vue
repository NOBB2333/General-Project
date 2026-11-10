<script setup lang="ts">
import { customFormsModel } from '@/features/system-settings/model/custom-forms.model';
import SystemSettingsListPage from './SystemSettingsListPage.vue';
</script>

<template>
  <SystemSettingsListPage :model="customFormsModel" />
</template>
