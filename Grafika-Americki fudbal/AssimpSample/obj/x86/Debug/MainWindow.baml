   M S B A M L   `   `   `  ����R  NSharpGL.WPF, Version=2.3.0.1, Culture=neutral, PublicKeyToken=27fc851303210b27>.clr-namespace:SharpGL.WPF;assembly=SharpGL.WPFSharpGL.WPF  5   	    AssimpSample   AssimpSample.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   BsharpGL.clr-namespace:SharpGL.WPF;assembly=SharpGL.WPF     5   	   -   5          � Title$  SharpGL WPF Application��5   	   $	��350��6)   $	��525��66   .��5      �  � RowDefinitions 5   
   �� 5      $	��30* �6   $��30��6)   5
      �� 5      $
��289* �6   5      .+�5   
      SharpGL.WPF.OpenGLControl  -   5        $��openGLControl��5      $��1��6H      DrawFPS .    RenderContextType$	 FBO=�5         	FrameRate$ 60Q�64   c� 5   
   -   5         c�Name$ toolBar��5      $��Stretch=�6#   $��0��6B   $	��Top=�6O   $��30��6h   $��30��6t   .@�5      �� $��Udaljenost gola��6   $��25��6.   $	��Top=�6:   �� 5      -   5         ��Name$
 dist��5      $��25��6#   $	��Top=�6/   $	��100��6G    � Minimum$
 -150Q�6t    � Maximum$	 190Q�6�   	 � Value$		 190Q�6�   
 � LargeChange$	
 0.1Q�6�   �� 5      $��Velicina lopte��6   $��25��6-   $	��Top=�69   �� 5      -   5        $
 size��5      $��25��6#   $	��Top=�6/   $	��100��6G   $
 0.25Q�6t   $ 2Q�6�   $	 1Q�6�   $	
 0.1Q�6�   �� 5      $��Brzina animacije��6   $��25��60   $	��Top=�6<   �� 5      -   5        $ speed��5      $��25��6$   $	��Top=�60   $	��100��6H   $	 0.1Q�6v   $	 1.5Q�6�   $		 0.5Q�6�   $	
 0.1Q�6�   5       5"      5#      