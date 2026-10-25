#!/usr/bin/env python3
# Unity の Editor.log にある最新の Build Report を読み、種類別サイズ・フォルダ別サイズ・画像の上位・
# 出力ファイルの大きさをまとめて表示する（読み取り専用）。
# 使い方: python3 tools/build-report-summary.py [Editor.log のパス] [ビルド出力フォルダ]
#   Editor.log の既定値: ~/Library/Logs/Unity/Editor.log
#   ビルド出力フォルダ（省略可）: 例 ~/necotapu_testbuild_C  （中の Build/ 以下の大きさを表示）
import os
import re
import sys
from collections import Counter

log = os.path.expanduser(sys.argv[1]) if len(sys.argv) > 1 else os.path.expanduser('~/Library/Logs/Unity/Editor.log')
out_dir = os.path.expanduser(sys.argv[2]) if len(sys.argv) > 2 else None
lines = open(log, encoding='utf-8', errors='ignore').read().split('\n')

starts = [i for i, l in enumerate(lines) if l == 'Build Report']
if not starts:
    sys.exit('Build Report が見つかりません: ' + log)
i = starts[-1]
print(f'Editor.log: {log}（Build Report は {len(starts)} 件。最後の 1 件を読みます）')

UNIT = {'kb': 1024, 'mb': 1048576, 'b': 1}
j = i + 1
print('--- 種類別（圧縮前）---')
while j < len(lines) and not lines[j].startswith('Used Assets'):
    if lines[j].strip():
        print(lines[j])
    j += 1
j += 1
items = []
while j < len(lines):
    m = re.match(r'^\s*([\d.]+) (kb|mb|b)\s+([\d.]+)% (.*)$', lines[j])
    if not m:
        break
    items.append((float(m.group(1)) * UNIT[m.group(2)], m.group(4)))
    j += 1
print(f'--- アセット {len(items)} 件、合計 {sum(v for v, _ in items) / 1048576:.1f}MB ---')

def pack(p):
    parts = p.split('/')
    if p.startswith(('Assets/AssetsOld/', 'Assets/AssetsForItems/')):
        return '/'.join(parts[:3])
    if p.startswith('Assets/'):
        return '/'.join(parts[:2])
    return parts[0]

by = Counter()
for v, p in items:
    by[pack(p)] += v
print('--- フォルダ別の上位 12 ---')
for k, v in by.most_common(12):
    print(f'{v / 1048576:7.1f}MB  {k}')

imgs = [(v, p) for v, p in items if p.lower().endswith(('.png', '.tga', '.tif', '.tiff', '.jpg', '.jpeg', '.psd', '.exr'))]
print(f'--- 画像 {len(imgs)} 件、合計 {sum(v for v, _ in imgs) / 1048576:.1f}MB。上位 15 ---')
for v, p in sorted(imgs, reverse=True)[:15]:
    print(f'{v / 1048576:7.2f}MB  {p}')

models = [(v, p) for v, p in items if p.lower().endswith(('.fbx', '.obj', '.blend', '.dae'))]
print(f'--- モデル {len(models)} 件、合計 {sum(v for v, _ in models) / 1048576:.1f}MB。上位 8 ---')
for v, p in sorted(models, reverse=True)[:8]:
    print(f'{v / 1048576:7.2f}MB  {p}')

if out_dir:
    print('--- 出力ファイルの大きさ（バイト）---')
    total = 0
    build = os.path.join(out_dir, 'Build')
    for f in sorted(os.listdir(build)):
        s = os.path.getsize(os.path.join(build, f))
        total += s
        print(f'{s:>12,}  {f}')
    print(f'{total:>12,}  合計')
