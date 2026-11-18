import { expect, test } from 'vitest';
import { formatarTamanho, tituloDoArquivo, validarArquivo } from './arquivos.js';

test('tituloDoArquivo tira a extensão e limpa separadores', () => {
  expect(tituloDoArquivo('Gearhole Prison - DM Version.jpg')).toBe('Gearhole Prison DM Version');
  expect(tituloDoArquivo('mapa_da_ilha.webp')).toBe('mapa da ilha');
  expect(tituloDoArquivo("Stahlmasts Lair - PC Version.JPG")).toBe('Stahlmasts Lair PC Version');
  expect(tituloDoArquivo('semextensao')).toBe('semextensao');
});

test('validarArquivo avisa tipo e tamanho antes de enviar', () => {
  expect(validarArquivo({ type: 'image/jpeg', size: 1000 })).toBeNull();
  expect(validarArquivo({ type: 'application/pdf', size: 1000 })).toBe('Envie uma imagem JPEG, PNG, WebP ou GIF.');
  expect(validarArquivo({ type: 'image/png', size: 15 * 1024 * 1024 + 1 })).toBe('A imagem pode ter até 15 MB.');
});

test('formatarTamanho', () => {
  expect(formatarTamanho(475136)).toBe('464 KB');
  expect(formatarTamanho(12178842)).toBe('11,6 MB');
  expect(formatarTamanho(512)).toBe('1 KB');
});
