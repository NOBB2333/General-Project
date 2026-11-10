<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { paymentRegisterModel } from '@/features/engineering-projects/model/payment-register.model';
</script>

<template>
  <ReferenceListPageSurface :model="paymentRegisterModel" />
</template>
