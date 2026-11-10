#!/usr/bin/env node
import { readFileSync, readdirSync } from 'node:fs';
import { extname, join, relative, resolve } from 'node:path';

const root = resolve(import.meta.dirname, '..');
const sourceRoot = join(root, 'src');
const errors = [];

const primitivePackages = new Set(['reka-ui', 'cmdk-vue', 'vue-sonner', 'vaul-vue']);

function collectSourceFiles(directory) {
  return readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const path = join(directory, entry.name);
    if (entry.isDirectory()) return collectSourceFiles(path);
    return ['.ts', '.vue'].includes(extname(entry.name)) ? [path] : [];
  });
}

function importSpecifiers(source) {
  return [
    ...source.matchAll(/\bfrom\s+['"]([^'"]+)['"]/g),
    ...source.matchAll(/\bimport\s*\(\s*['"]([^'"]+)['"]\s*\)/g),
  ].map((match) => match[1]);
}

function fail(file, message) {
  errors.push(`${relative(root, file)}: ${message}`);
}

for (const file of collectSourceFiles(sourceRoot)) {
  const source = readFileSync(file, 'utf8');
  const name = relative(sourceRoot, file).replaceAll('\\', '/');
  const imports = importSpecifiers(source);

  if (name.startsWith('features/')) {
    for (const specifier of imports) {
      const packageName = specifier.startsWith('@')
        ? specifier.split('/').slice(0, 2).join('/')
        : specifier.split('/')[0];
      if (primitivePackages.has(packageName)) {
        fail(file, `feature 禁止直接依赖 ${packageName}；请使用 @/components/ui 内部封装`);
      }
      if (specifier.startsWith('@/components/ui/')) {
        fail(file, 'feature 必须从 @/components/ui barrel 导入，禁止绕过内部组件公开接口');
      }
      if (specifier.startsWith('@/components/page/scaffold')) {
        fail(file, 'feature 禁止直接依赖 page/scaffold 内部实现；旧脚手架只能经 @/components/page 兼容入口使用');
      }
    }
    if (/\b(?:window\.)?history\.(?:pushState|replaceState)\s*\(/.test(source)) {
      fail(file, '业务页面禁止直接操作 browser history；请使用 Vue Router');
    }
  }

  if (name.startsWith('components/ui/')) {
    for (const specifier of imports) {
      if (
        specifier.startsWith('@/features/') ||
        specifier.startsWith('@/components/page') ||
        specifier.startsWith('@/components/layout') ||
        specifier.startsWith('@/router')
      ) {
        fail(file, `components/ui 不得反向依赖上层模块 ${specifier}`);
      }
    }
  }

  if (name.startsWith('components/page/')) {
    for (const specifier of imports) {
      if (
        specifier.startsWith('@/features/') ||
        specifier.startsWith('@/components/layout') ||
        specifier.startsWith('@/router')
      ) {
        fail(file, `components/page 不得反向依赖上层模块 ${specifier}`);
      }
    }
  }

  if (name.startsWith('components/layout/')) {
    for (const specifier of imports) {
      if (specifier.startsWith('@/features/')) {
        fail(file, `components/layout 不得反向依赖业务 feature ${specifier}`);
      }
    }
  }
}

if (errors.length > 0) {
  console.error(errors.join('\n'));
  process.exit(1);
}

console.log('Architecture dependency boundary audit passed');
