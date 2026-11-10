import { readdirSync, readFileSync } from 'node:fs';
import { extname, join, relative, resolve } from 'node:path';

const root = resolve(import.meta.dirname, '..');
const sourceRoot = join(root, 'src');

function collectVueFiles(directory) {
  return readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const path = join(directory, entry.name);
    if (entry.isDirectory()) return collectVueFiles(path);
    return extname(entry.name) === '.vue' ? [path] : [];
  });
}

const violations = [];
const allowedGlobalClassUsage = new Map([
  ['features/sales/views/SalesCheckinPage.vue', new Set(['success', 'danger'])],
  ['features/service/views/ServiceAnalyticsPage.vue', new Set(['success', 'purple'])],
]);

for (const file of collectVueFiles(sourceRoot)) {
  const source = readFileSync(file, 'utf8');
  const name = relative(root, file);
  const template = source.match(/<template>[\s\S]*<\/template>/)?.[0] ?? '';
  const checks = [
    { pattern: /(?<!:)\bstyle="|:style=/g, message: '禁止行内 style / :style' },
    {
      pattern: /:class="[^"]*['"][^'"]*(?:bg-|text-|border|w-\[|h-\[|opacity-|translate-|rotate-|hover:|ring-)/g,
      message: '动态 class 中包含 Tailwind utility 字符串',
    },
  ];
  for (const check of checks) {
    if (check.pattern.test(template)) violations.push(`${name}: ${check.message}`);
  }
  const allowedClasses = allowedGlobalClassUsage.get(relative(sourceRoot, file));
  for (const match of template.matchAll(/(?<![-:])\bclass="([^"]*)"/g)) {
    const classNames = match[1].trim().split(/\s+/).filter(Boolean);
    if (classNames.some((className) => !allowedClasses?.has(className))) {
      violations.push(`${name}: 模板包含未登记的静态 class；请改用 :class="$style.semanticName"`);
    }
  }
  for (const binding of template.matchAll(/:class="([^"]*)"/g)) {
    if (!binding[1].includes('$style')) {
      violations.push(`${name}: :class 必须引用同文件的 $style 语义类`);
    }
  }
  if (template.includes('$style.') && !source.includes('<style module>')) {
    violations.push(`${name}: 使用了 $style 但缺少 <style module>`);
  }
  if (source.includes('<style module>')) {
    // Depth-aware: features/* needs ../../styles; features/*/pages/* needs ../../../styles
    const refMatch = source.match(/<style module>\s*@reference "((?:\.\.\/)+styles\/index\.css)";/);
    if (!refMatch) {
      violations.push(`${name}: <style module> 必须首先 @reference 相对路径到 styles/index.css`);
    } else {
      const resolved = resolve(file, '..', refMatch[1]);
      if (resolved !== resolve(sourceRoot, 'styles/index.css')) {
        violations.push(`${name}: @reference 未解析到 src/styles/index.css (got ${refMatch[1]})`);
      }
    }
  }
}

if (violations.length) {
  console.error(violations.join('\n'));
  process.exit(1);
}

console.log('Vue semantic CSS Module audit passed');
