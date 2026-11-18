import { expect, test } from 'vitest';
import { formatarTamanho, tituloDoArquivo, validarArquivo, nomeArquivoDownload, dimensoesParaCopiar } from './arquivos.js';

test('tituloDoArquivo tira a extensão e limpa separadores', () => {
  expect(tituloDoArquivo('Prisao do Porto - DM Version.jpg')).toBe('Prisao do Porto DM Version');
  expect(tituloDoArquivo('mapa_da_ilha.webp')).toBe('mapa da ilha');
  expect(tituloDoArquivo("Covil do Inventor - PC Version.JPG")).toBe('Covil do Inventor PC Version');
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

test('nomeArquivoDownload: limpa o título e põe a extensão do tipo', () => {
  expect(nomeArquivoDownload('Retrato: o capitão?', 'image/jpeg')).toBe('Retrato o capitão.jpg');
  expect(nomeArquivoDownload('mapa/porto', 'image/png')).toBe('mapa porto.png');
  expect(nomeArquivoDownload('  ', 'image/webp')).toBe('imagem.webp');
  expect(nomeArquivoDownload('animado', 'image/gif')).toBe('animado.gif');
});

test('dimensoesParaCopiar: reduz para caber em 2048 px no lado maior, sem aumentar as pequenas', () => {
  expect(dimensoesParaCopiar(5120, 5120)).toEqual({ largura: 2048, altura: 2048 });
  expect(dimensoesParaCopiar(4000, 2000)).toEqual({ largura: 2048, altura: 1024 });
  expect(dimensoesParaCopiar(1000, 3000)).toEqual({ largura: 683, altura: 2048 });
  expect(dimensoesParaCopiar(800, 600)).toEqual({ largura: 800, altura: 600 });
});
