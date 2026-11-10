import type { CustomerMaterialDraft } from '@/features/crm/model/customer-materials.types';

function parseCsvLine(line: string) {
  const values: string[] = [];
  let value = '';
  let quoted = false;
  for (let index = 0; index < line.length; index += 1) {
    const character = line[index];
    if (character === '"' && quoted && line[index + 1] === '"') {
      value += '"';
      index += 1;
    } else if (character === '"') {
      quoted = !quoted;
    } else if (character === ',' && !quoted) {
      values.push(value.trim());
      value = '';
    } else {
      value += character;
    }
  }
  values.push(value.trim());
  return values;
}

export function parseCustomerMaterialCsv(source: string): CustomerMaterialDraft[] {
  const lines = source
    .replace(/^\uFEFF/, '')
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean);
  if (lines.length < 2) return [];

  const headers = parseCsvLine(lines[0]);
  const column = (...names: string[]) => names.map((name) => headers.indexOf(name)).find((index) => index >= 0) ?? -1;
  const indexes = {
    customer: column('客户名称', '客户'),
    internalCode: column('我方物料编码'),
    internalName: column('我方物料名称'),
    internalSpec: column('规格型号', '我方物料规格'),
    unit: column('单位'),
    customerCode: column('客户物料编码'),
    customerName: column('客户物料名称'),
    remark: column('备注'),
  };
  if (
    indexes.customer < 0 ||
    indexes.internalCode < 0 ||
    indexes.internalName < 0 ||
    indexes.customerCode < 0 ||
    indexes.customerName < 0
  ) {
    return [];
  }

  return lines.slice(1).flatMap((line) => {
    const values = parseCsvLine(line);
    const customer = values[indexes.customer]?.trim();
    const internalCode = values[indexes.internalCode]?.trim();
    const internalName = values[indexes.internalName]?.trim();
    const customerCode = values[indexes.customerCode]?.trim();
    const customerName = values[indexes.customerName]?.trim();
    if (!customer || !internalCode || !internalName || !customerCode || !customerName) return [];
    return [
      {
        customer,
        internalCode,
        internalName,
        internalSpec: values[indexes.internalSpec]?.trim() || '-',
        unit: values[indexes.unit]?.trim() || '台',
        customerCode,
        customerName,
        remark: values[indexes.remark]?.trim() || '-',
      },
    ];
  });
}
