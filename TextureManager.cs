using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GameOpenGL
{
    public static class TextureManager
    {
        private static readonly Dictionary<string, int> _textures = new Dictionary<string, int>();
        
        public static int LoadTexture(string filename)
        {
            // Если текстура уже загружена, возвращаем её ID
            if (_textures.TryGetValue(filename, out int id))
                return id;
                
            // Генерируем новый ID текстуры
            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            
            // Загружаем изображение
            using (var bitmap = new Bitmap(filename))
            {
                // Переворачиваем изображение (в OpenGL (0,0) - левый нижний угол)
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                
                // Получаем данные изображения
                BitmapData data = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                
                // Передаем данные в OpenGL
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 
                    data.Width, data.Height, 0, 
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, 
                    PixelType.UnsignedByte, data.Scan0);
                
                bitmap.UnlockBits(data);
            }
            
            // Настройки фильтрации текстуры
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            // Сохраняем ID текстуры
            _textures[filename] = id;
            return id;
        }
    }
}