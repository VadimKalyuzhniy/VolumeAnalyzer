   M S B A M L   `   `   `  ���� K  GStockSharp.Xaml, Version=4.1.18.0, Culture=neutral, PublicKeyToken=null\?clr-namespace:StockSharp.Xaml.Charting;assembly=StockSharp.XamlStockSharp.Xaml.Charting   WpfApplication1    WpfApplication1.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Tcharting?clr-namespace:StockSharp.Xaml.Charting;assembly=StockSharp.Xaml       � Title$  
DartWeider��$��638.807��$��1063.508�� � 
ResizeMode$ CanMinimize=� � Icon$* $20130120035232541_easyicon_cn_48.ico�� � WindowStartupLocation$ CenterScreen=�����  �� EndPoint$ 0.5,1&� �� 
StartPoint$ 0.5,0&�.���  � Color$ Black�� � Offset$ 1Q�� $ White��.��� $��
15,15,19,0q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto �	 � ColumnDefinitions	 �� $
��1024 �.+�� $��0��$	��150��.+��� -   
 ��Name$
 quikPath��$
��Left=�$��25��$��
127,19,0,0q� �� TextWrapping$
 Wrap=�$%��Введите путь к Quik��$	��Top=�$	��301�� J� RenderTransformOrigin$ 0.582,0.426&��� $��Путь к Quik:��$
��Left=�$��	10,19,0,0q�$	��Top=�$ 0.132,0.154&�$��91��$��25���� -    ��Name$ 
connButton��$��Подключиться��$
��Left=�$��
447,22,0,0q�$	��Top=�$��93��$��64���� -    ��Name$ userPortfolios��$
��Left=�$��
127,63,0,0q�$	��Top=�$	��121��$��25���� -   $ userSecurities��$
��Left=�$��127,102,0,0q�$	��Top=�$	��121��$��25���� $��Портфель:��$
��Left=�$��	10,63,0,0q�$	��Top=�$ -0.211,-0.077&��� $��Инструмент:��$
��Left=�$��
10,101,0,0q�$	��Top=��� $��Таймфрейм:��$
��Left=�$��
263,63,0,0q�$	��Top=�$��87���� -   $
 userTimeFrame��$
��Left=�$��23��$��277,102,0,0q�$
 Wrap=�$��60��$	��Top=�$��41���� -   $ StrategyStartButton��$%��Запуск стратегии��$
��Left=�$��
557,22,0,0q�$	��Top=�$	��106��$��64���� -    ��Name$ LogWind��$
��Left=�$	��426��$��331,154,0,-430q�$	��Top=�$	��332�� �� 
IsReadOnly . " �� VerticalScrollBarVisibility$ Visible=� � ContextMenu �� . �/� �� $��Шаг:��$
��Left=�$��
355,64,0,0q�$	��Top=�$��73���� -   $
 userStep��$
��Left=�$��23��$��355,102,0,0q�$
 Wrap=�$��10��$	��Top=�$��41���� -	   $ LogWind2��$
��Left=�$	��561��$��682,19,0,-430q�$	��Top=�$	��332�� . $ Visible=� �� . �/� 