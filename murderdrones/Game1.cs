using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace murderdrones
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _playButtonTexture; // Textura del botón Play
        private Texture2D _fondoButtonTexture; // Textura de fondo para el botón
        private Rectangle _playButtonBounds; // Rectángulo para detectar clics en el botón
        private Vector2 _playButtonPosition; // Posición del botón Play
        private float _playButtonScale = 0.5f; // Escala del botón Play

        private Vector2 _fondoButtonPosition; // Posición de la imagen de fondo del botón
        private float _fondoButtonScale = 0.7f; // Escala para la imagen de fondo del botón

        private Texture2D _marioTexture;
        private Texture2D _platformTexture;
        private Texture2D _backgroundTexture;
        private Texture2D _centinelaTexture;
        private Texture2D _metalPlatformTexture;

        private Vector2 _marioPosition;
        private Vector2 _platformPosition;
        private Vector2 _centinelaPosition;
        private Vector2 _metalPlatformPosition;

        private Vector2 _marioVelocity;
        private float _gravity = 0.5f;
        private bool _isJumping = false;

        private float _marioScale = 0.090f;
        private float _centinelaScale = 0.12f;
        private float _metalPlatformScale = 0.050f;
        private int _platformRepeatCount = 5;
        private int _backgroundRepeatCount;

        private Matrix _cameraTransform;
        private Vector2 _cameraPosition;

        private bool _gameStarted = false; // Indica si el juego ha comenzado

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _platformPosition = new Vector2(0, 430);
            _marioPosition = new Vector2(100, _platformPosition.Y - _marioScale * 100);
            _centinelaPosition = new Vector2(1380, _platformPosition.Y - 110);
            _metalPlatformPosition = new Vector2(600, 200);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _playButtonTexture = Content.Load<Texture2D>("play_button"); // Cargar la textura del botón Play
            _fondoButtonTexture = Content.Load<Texture2D>("fondo_button"); // Cargar la textura del fondo del botón
            _marioTexture = Content.Load<Texture2D>("mario");
            _platformTexture = Content.Load<Texture2D>("plataforma");
            _backgroundTexture = Content.Load<Texture2D>("fondo");
            _centinelaTexture = Content.Load<Texture2D>("centinela");
            _metalPlatformTexture = Content.Load<Texture2D>("metal");

            _backgroundRepeatCount = (_platformTexture.Width * _platformRepeatCount) / _backgroundTexture.Width + 1;

            // Centralizar el botón Play y la imagen de fondo
            int screenWidth = _graphics.PreferredBackBufferWidth;
            int screenHeight = _graphics.PreferredBackBufferHeight;

            _playButtonPosition = new Vector2(
                (screenWidth - _playButtonTexture.Width * _playButtonScale) / 2,
                (screenHeight - _playButtonTexture.Height * _playButtonScale) / 2
            );

            _fondoButtonPosition = new Vector2(
                (screenWidth - _fondoButtonTexture.Width * _fondoButtonScale) / 2,
                (screenHeight - _fondoButtonTexture.Height * _fondoButtonScale) / 2
            );

            _playButtonBounds = new Rectangle(
                (int)_playButtonPosition.X,
                (int)_playButtonPosition.Y,
                (int)(_playButtonTexture.Width * _playButtonScale),
                (int)(_playButtonTexture.Height * _playButtonScale)
            );
        }

        protected override void Update(GameTime gameTime)
        {
            if (!_gameStarted)
            {
                // Detectar clic en el botón Play
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    Point mousePosition = Mouse.GetState().Position;
                    if (_playButtonBounds.Contains(mousePosition))
                    {
                        _gameStarted = true; // Iniciar el juego
                    }
                }
                return; // Detener el resto de la lógica mientras no inicie el juego
            }

            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Right))
                _marioPosition.X += 3;
            if (keyboardState.IsKeyDown(Keys.Left))
                _marioPosition.X -= 3;

            // Aumentar la altura del salto para llegar a la plataforma de metal
            if (keyboardState.IsKeyDown(Keys.Space) && !_isJumping)
            {
                _marioVelocity.Y = -15; // Aumentar el valor negativo para saltar más alto
                _isJumping = true;
            }

            _marioVelocity.Y += _gravity;
            _marioPosition += _marioVelocity;

            // Verificación de colisión con la plataforma base
            if (_marioPosition.Y + _marioTexture.Height * _marioScale >= _platformPosition.Y &&
                _marioPosition.X + _marioTexture.Width * _marioScale >= _platformPosition.X &&
                _marioPosition.X <= _platformPosition.X + _platformTexture.Width * _platformRepeatCount)
            {
                _marioVelocity.Y = 0;
                _isJumping = false;
                _marioPosition.Y = _platformPosition.Y - _marioTexture.Height * _marioScale;
            }

            // Verificación de colisión con la plataforma 'metal'
            if (_marioPosition.Y + _marioTexture.Height * _marioScale >= _metalPlatformPosition.Y &&
                _marioPosition.X + _marioTexture.Width * _marioScale >= _metalPlatformPosition.X &&
                _marioPosition.X <= _metalPlatformPosition.X + _metalPlatformTexture.Width * _metalPlatformScale)
            {
                if (_marioVelocity.Y >= 0)
                {
                    _marioVelocity.Y = 0;
                    _isJumping = false;
                    _marioPosition.Y = _metalPlatformPosition.Y - _marioTexture.Height * _marioScale;
                }
            }

            // Mover el Centinela hacia la izquierda
            _centinelaPosition.X -= 2;

            if (_centinelaPosition.X < 0)
                _centinelaPosition.X = 1380;

            if (CheckCollision(_marioPosition, _marioTexture, _centinelaPosition, _centinelaTexture))
            {
                Exit(); // Salir del juego en caso de colisión
            }

            _cameraPosition = new Vector2(_marioPosition.X - _graphics.PreferredBackBufferWidth / 2, 0);
            float levelWidth = _platformTexture.Width * _platformRepeatCount;
            _cameraPosition.X = Math.Max(0, Math.Min(_cameraPosition.X, levelWidth - _graphics.PreferredBackBufferWidth));
            _cameraTransform = Matrix.CreateTranslation(-_cameraPosition.X, -_cameraPosition.Y, 0);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            if (!_gameStarted)
            {
                // Dibujar la imagen de fondo del botón
                _spriteBatch.Draw(
                    _fondoButtonTexture,
                    _fondoButtonPosition,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    _fondoButtonScale,
                    SpriteEffects.None,
                    0f
                );

                // Dibujar el botón Play encima del fondo
                _spriteBatch.Draw(
                    _playButtonTexture,
                    _playButtonPosition,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    _playButtonScale,
                    SpriteEffects.None,
                    0f
                );
            }
            else
            {
                _spriteBatch.End();
                _spriteBatch.Begin(transformMatrix: _cameraTransform);

                for (int i = 0; i < _backgroundRepeatCount; i++)
                {
                    _spriteBatch.Draw(_backgroundTexture, new Vector2(i * _backgroundTexture.Width, 0), Color.White);
                }

                for (int i = 0; i < _platformRepeatCount; i++)
                {
                    _spriteBatch.Draw(_platformTexture, new Vector2(_platformPosition.X + i * _platformTexture.Width, _platformPosition.Y), Color.White);
                }

                _spriteBatch.Draw(_metalPlatformTexture, _metalPlatformPosition, null, Color.White, 0f, Vector2.Zero, _metalPlatformScale, SpriteEffects.None, 0f);
                _spriteBatch.Draw(_marioTexture, _marioPosition, null, Color.White, 0f, Vector2.Zero, _marioScale, SpriteEffects.None, 0f);
                _spriteBatch.Draw(_centinelaTexture, _centinelaPosition, null, Color.White, 0f, Vector2.Zero, _centinelaScale, SpriteEffects.None, 0f);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private bool CheckCollision(Vector2 marioPosition, Texture2D marioTexture, Vector2 centinelaPosition, Texture2D centinelaTexture)
        {
            Rectangle marioRect = new Rectangle((int)marioPosition.X, (int)marioPosition.Y, (int)(marioTexture.Width * _marioScale), (int)(marioTexture.Height * _marioScale));
            Rectangle centinelaRect = new Rectangle((int)centinelaPosition.X, (int)centinelaPosition.Y, (int)(centinelaTexture.Width * _centinelaScale), (int)(centinelaTexture.Height * _centinelaScale));
            return marioRect.Intersects(centinelaRect);
        }
    }
}