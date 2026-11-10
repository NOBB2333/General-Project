import type { CustomerDraft, CustomerLevel, CustomerType } from '@/features/crm/model/customers.types';

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

function isCustomerType(value: string | undefined): value is CustomerType {
  return value === '终端客户' || value === '集成商' || value === 'EPC';
}

function isCustomerLevel(value: string | undefined): value is CustomerLevel {
  return (
    value === 'S级（战略客户）' ||
    value === 'A级（重要客户）' ||
    value === 'B级（一般客户）' ||
    value === 'C级（小客户）'
  );
}

export function parseCustomerCsv(source: string): CustomerDraft[] {
  const lines = source
    .replace(/^\uFEFF/, '')
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean);
  if (lines.length < 2) return [];

  const headers = parseCsvLine(lines[0]);
  const column = (...names: string[]) => names.map((name) => headers.indexOf(name)).find((index) => index >= 0) ?? -1;
  const indexes = {
    name: column('客户名称', '名称'),
    type: column('客户类型', '类型'),
    level: column('客户级别', '级别'),
    owner: column('负责人'),
    contact: column('联系人', '主要联系人'),
    phone: column('联系电话', '电话', '手机号'),
    email: column('邮箱'),
    address: column('地址', '详细地址'),
    creditLimit: column('信用额度'),
    paymentDays: column('账期', '账期天数'),
  };
  if (indexes.name < 0 || indexes.contact < 0 || indexes.phone < 0) return [];

  return lines.slice(1).flatMap((line) => {
    const values = parseCsvLine(line);
    const name = values[indexes.name]?.trim();
    const contact = values[indexes.contact]?.trim();
    const phone = values[indexes.phone]?.trim();
    if (!name || !contact || !phone) return [];
    const rawType = values[indexes.type]?.trim();
    const rawLevel = values[indexes.level]?.trim();
    return [
      {
        name,
        type: isCustomerType(rawType) ? rawType : '终端客户',
        level: isCustomerLevel(rawLevel) ? rawLevel : 'B级（一般客户）',
        owner: values[indexes.owner]?.trim() || '张伟',
        contact,
        phone,
        email: values[indexes.email]?.trim() || '',
        address: values[indexes.address]?.trim() || '',
        creditLimit: values[indexes.creditLimit]?.trim() || '300000',
        paymentDays: values[indexes.paymentDays]?.trim() || '30',
      },
    ];
  });
}
