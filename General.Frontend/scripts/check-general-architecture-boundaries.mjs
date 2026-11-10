import { readdirSync, readFileSync } from 'node:fs';
import { dirname, extname, relative, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const projectRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const sourceRoot = resolve(projectRoot, 'src');
const sourceExtensions = new Set(['.js', '.jsx', '.ts', '.tsx', '.vue']);
const forbiddenPackageNames = [
  '@ant-design/vue',
  '@ant-design-vue',
  '@vben',
  '@vben-core',
  '@vxe-table',
  '@general-web',
  'ant-design-vue',
  'general-web',
];
const primitivePackages = new Set(['cmdk-vue', 'lucide-vue-next', 'reka-ui', 'vue-sonner']);
const importPattern =
  /(?:\bimport\s+(?:[^'";]*?\s+from\s+)?|\bexport\s+[^'";]*?\s+from\s+|\bimport\s*\()\s*['"]([^'"]+)['"]/g;
const violations = [];

function collectSourceFiles(directory) {
  const files = [];

  for (const entry of readdirSync(directory, { withFileTypes: true })) {
    const path = resolve(directory, entry.name);
    if (entry.isDirectory()) {
      files.push(...collectSourceFiles(path));
    } else if (entry.isFile() && sourceExtensions.has(extname(entry.name))) {
      files.push(path);
    }
  }

  return files;
}

function relativeSourcePath(path) {
  return relative(sourceRoot, path).split(sep).join('/');
}

function packageName(specifier) {
  if (specifier.startsWith('@')) {
    return specifier.split('/').slice(0, 2).join('/');
  }

  return specifier.split('/')[0];
}

function isForbiddenPackage(name) {
  return forbiddenPackageNames.some((forbidden) => name === forbidden || name.startsWith(`${forbidden}/`));
}

function report(file, line, message) {
  violations.push(`${relative(projectRoot, file)}:${line} ${message}`);
}

function resolveLocalImport(file, specifier, line) {
  let target;

  if (specifier.startsWith('./') || specifier.startsWith('../')) {
    target = resolve(dirname(file), specifier);
  } else if (specifier.startsWith('@/')) {
    target = resolve(sourceRoot, specifier.slice(2));
  } else {
    return undefined;
  }

  const sourceRelativePath = relativeSourcePath(target);
  if (sourceRelativePath === '..' || sourceRelativePath.startsWith('../')) {
    report(file, line, `本地导入越出src目录：${specifier}`);
    return undefined;
  }

  return sourceRelativePath;
}

function checkLayerImport(file, targetPath, line, specifier) {
  const importer = relativeSourcePath(file);
  const importerFeature = importer.match(/^features\/([^/]+)\//)?.[1];
  const targetFeature = targetPath.match(/^features\/([^/]+)\//)?.[1];

  if (importer.startsWith('api/') && /^(features|components|router|stores)\//.test(targetPath)) {
    report(file, line, `api层不能依赖${targetPath}：${specifier}`);
  }

  if (importer.startsWith('components/ui/') && /^(api|composables|features|layout|router|stores)\//.test(targetPath)) {
    report(file, line, `components/ui不能依赖业务或应用层${targetPath}：${specifier}`);
  }

  if (
    importer.startsWith('components/page/') &&
    /^(api|composables|features|layout|router|stores)\//.test(targetPath)
  ) {
    report(file, line, `components/page不能依赖业务或应用层${targetPath}：${specifier}`);
  }

  if (importer.startsWith('components/layout/') && /^(api|features)\//.test(targetPath)) {
    report(file, line, `components/layout不能依赖业务或API层${targetPath}：${specifier}`);
  }

  if (importerFeature && targetFeature && importerFeature !== targetFeature) {
    report(file, line, `feature之间不能互相导入内部实现：${specifier}`);
  }
}

function checkPackageDependencies() {
  const packageJson = JSON.parse(readFileSync(resolve(projectRoot, 'package.json'), 'utf8'));
  const dependencies = {
    ...packageJson.dependencies,
    ...packageJson.devDependencies,
  };

  for (const dependencyName of Object.keys(dependencies)) {
    if (isForbiddenPackage(dependencyName)) {
      violations.push(`package.json: 禁止依赖旧前端包 ${dependencyName}`);
    }
  }
}

checkPackageDependencies();

for (const file of collectSourceFiles(sourceRoot)) {
  const source = readFileSync(file, 'utf8');
  let match;

  while ((match = importPattern.exec(source)) !== null) {
    const specifier = match[1];
    const line = source.slice(0, match.index).split('\n').length;
    const importedPackage = packageName(specifier);
    const importer = relativeSourcePath(file);

    if (isForbiddenPackage(importedPackage) || specifier.toLowerCase().includes('general.web/')) {
      report(file, line, `禁止引用旧前端依赖：${specifier}`);
      continue;
    }

    if (primitivePackages.has(importedPackage) && !importer.startsWith('components/ui/')) {
      report(file, line, `底层UI依赖只能由components/ui封装：${specifier}`);
    }

    const targetPath = resolveLocalImport(file, specifier, line);
    if (targetPath) {
      checkLayerImport(file, targetPath, line, specifier);
    }
  }
}

if (violations.length > 0) {
  console.error(`依赖边界检查失败（${violations.length}项）：`);
  for (const violation of violations) {
    console.error(`- ${violation}`);
  }
  process.exitCode = 1;
} else {
  console.log(`依赖边界检查通过：扫描${collectSourceFiles(sourceRoot).length}个源文件。`);
}
