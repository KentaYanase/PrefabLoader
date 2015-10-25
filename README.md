# PrefabLoader
UnityのPrefabを多層化や読み込みの分割を可能にするコンポーネントです。

ロードするプレハブはEditor上ではシーン上にプレビューが表示されるので編集もしやすいです。

# 使い方

## プレハブの登録
1. PrefabLoaderコンポーネントをアタッチ  
1. LoadTimingを<b>OnAwake / OnStart / OnExternal</b>から選択
1. PrefabInfoListの<b>+</b>ボタンを押して要素を追加.
1. LoadModeを選択する
 * Referenceモードではプレハブの参照で登録できる
 * Resource PathモードではResourcesフォルダパスで登録できる
 * AssetBundleモードは未実装.
1. <b>Instantiate</b>ボタンを押してプレビュー用オブジェクトを生成

## 生成するプレハブの座標を記録する

1. プレビュー用オブジェクトを移動させる
1. <b>Update TransformData</b>ボタンを押して、Transformを記録する

## 実行時のプレハブの生成
* OnAwake / OnStart ではAwake / Startのタイミングで自動的にロードされます。
* OnExternalでは他のスクリプトから、任意のタイミングでロードすることができます。

PrefabLoaderをつけたGameObjectはコンポーネントにある<b>Select/Revert/Apply</b>ボタンを利用すること.
標準のApplyボタンを使うとプレビュー用オブジェクトがプレハブに保存されてしまうので注意。
