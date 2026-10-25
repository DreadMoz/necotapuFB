#!/usr/bin/env node
// お題チェック用スクリプト。
// Assets/Resources/ 配下のお題（TextPrompts / TextChallange / TextCustom）に、
// フォントの文字セット（Assets/Fonts/NekotapuCharset.txt）にない文字がないかを調べて報告する。
// お題ファイルは変更しない。使い方：リポジトリルートで `node tools/check-odai.js`

const fs = require('fs');
const path = require('path');

const root = path.resolve(__dirname, '..');
const charsetPath = path.join(root, 'Assets', 'Fonts', 'NekotapuCharset.txt');
const odaiDirs = ['TextPrompts', 'TextChallange', 'TextCustom'];
const skipFiles = new Set(['0fileList.json']);

if (!fs.existsSync(charsetPath)) {
  console.error('文字セットのファイルが見つかりません：' + charsetPath);
  process.exit(2);
}
const allowed = new Set(Array.from(fs.readFileSync(charsetPath, 'utf8')));

// 1つのお題ファイルから、表示される文字列を場所つきで取り出す。
function collectTexts(data) {
  const texts = [];
  for (const key of ['title', 'description']) {
    if (data[key] !== undefined) texts.push({ where: key, text: String(data[key]), line: '' });
  }
  for (const m of data.messages || []) {
    texts.push({ where: `messages(count=${m.count})`, text: String(m.description ?? ''), line: '' });
  }
  for (const t of data.themes || []) {
    const line = `${t.kanji ?? ''}（${t.hiragana ?? ''}）`;
    texts.push({ where: `id=${t.id} kanji`, text: String(t.kanji ?? ''), line });
    texts.push({ where: `id=${t.id} hiragana`, text: String(t.hiragana ?? ''), line });
  }
  return texts;
}

const problems = []; // { file, where, ch, line }
const broken = [];   // 読み込めなかったファイル
let fileCount = 0;

for (const dir of odaiDirs) {
  const dirPath = path.join(root, 'Assets', 'Resources', dir);
  if (!fs.existsSync(dirPath)) continue;
  const files = fs.readdirSync(dirPath).filter((f) => f.endsWith('.json') && !skipFiles.has(f)).sort();
  for (const f of files) {
    const rel = `${dir}/${f}`;
    let data;
    try {
      data = JSON.parse(fs.readFileSync(path.join(dirPath, f), 'utf8'));
    } catch (e) {
      broken.push({ file: rel, message: e.message });
      continue;
    }
    fileCount++;
    for (const { where, text, line } of collectTexts(data)) {
      for (const ch of text) {
        if (!allowed.has(ch)) problems.push({ file: rel, where, ch, line: line || text });
      }
    }
  }
}

const hex = (ch) => 'U+' + ch.codePointAt(0).toString(16).toUpperCase().padStart(4, '0');

if (broken.length > 0) {
  console.log('【よめなかったファイル】JSON の書き方にまちがいがあるようです。');
  for (const b of broken) console.log(`  ${b.file} … ${b.message}`);
  console.log('');
}

if (problems.length === 0 && broken.length === 0) {
  console.log(`すべてのお題が表示できます。（${fileCount} ファイルをチェックしました）`);
  process.exit(0);
}

if (problems.length > 0) {
  const kinds = new Set(problems.map((p) => p.ch));
  console.log(`使えない文字が ${kinds.size} しゅるい、${problems.length} か所 見つかりました。（${fileCount} ファイルをチェック）`);
  console.log('');
  for (const p of problems) {
    console.log(`${p.file}  ${p.where}  「${p.ch}」(${hex(p.ch)})  お題：${p.line}`);
  }
}
process.exit(1);
