   M S B A M L   `   `   `  ����B  >UnBox3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:UnBox3D.ViewModelsUnBox3D.ViewModels  5   	       UnBox3D.Views.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   7
viewmodels clr-namespace:UnBox3D.ViewModels     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	     � Title$  UnBox3D��5   	   $	��600��6   $	��800��6&    � WindowStartupLocation$ CenterScreen=�5   	   .��5      ]� .+�5
   
   o� $	��Top=�6   .@�5      m� $
��File��6   .@�5      m� $��Import Model��6    m� Command 61   �� *ImportObjModelCommand+m� 5      $��Simplify Model��6   .@�5      m� $��QEM Simplification��6    6;   �� *SimplifyQEMCommand+m� 5      $��Edge Collapse��6    66   �� *SimplifyEdgeCollapseCommand+m� 5      $��
Decimation��6    63   �� *SimplifyDecimationCommand+m� 5      $��Adaptive Decimation��6    6<   �� *#!SimplifyAdaptiveDecimationCommand+5      m� 5      $��Export Model��6    61   �� *ExportMeshCommand+m� 5      $
��Exit��6    6)   �� *ExitCommand+5      m� 5      $
��View��6   .@�5      m� $��
Reset View��6    6/   �� *ResetViewCommand+5      m� 5      $��Settings��6   m� 5      $��Tools��6   .@�5      m� $��Hierarchy Panel��6   5      m� 5      $��About Us��6   .@�5      m� $��About��6    6*   �� *AboutCommand+5      5       � 5#   
    � RowDefinitions 5$      �� 5%      $��* �6     � ColumnDefinitions 5'      �� 5(      $
��Auto �6#   $	��150��60   $	��400��6?   �� 5)      $��5 �6#   �� 5*      $��* �6#   .+�5.      R� $��0��6   $��10q�6'   ��64   �� *Meshes+��5/      �� 50       �� ItemsSource 6/   �� *
Vertices+.R�52      ��  �� Orientation$ 
Horizontal=�6%   .+�53      �� $
��Bold"�6>   ��6(   �� *Name+ � ContextMenu 54   "   �� 55   &   .@�56   *   m� $��Rename��63   m� 57   *   $��Replace with...��63   .@�58   .   m� $	��Box��67   m� 59   .   $��	Rectangle��67   m� 5:   .   $��Cylinder��67   m� 5;   .   $��Plane��67   5<   +   m� 5=   *   $��Simplify Mesh��63   .@�5>   .   m� $��QEM Simplification��67   m� 5?   .   $��Edge Collapse��67   m� 5@   .   $��
Decimation��67   m� 5A   .   $��Adaptive Decimation��67   5B   +   m� 5C   *   $��Delete��63   5D   '   5F      �� 5G      $�� (��6(   
�������62   �� 5H      
���������6(   �� *VertexCount+6E   �� 5I      $��
 vertices)��6(   
�������6:   5J       �� ItemTemplate 5M      �� 5N      .R�5O   "   �� ��6,   �� 5P      5R      5T      �� 5V      $��1��6   $��5��5W      $��Stretch=�5X      
�������	 �� ShowsPreview$
	 True��5Z      7  1System.Windows.Forms.Integration.WindowsFormsHost  5\      -   5        $��
openGLHost��5\   /   $��2��6   6/   5]      5^      5_      