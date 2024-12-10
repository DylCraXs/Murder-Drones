using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace murderdrons
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _playButtonTexture;
        private Texture2D _fondoButtonTexture;
        private Rectangle _playButtonBounds;
        private Vector2 _playButtonPosition;
        private float _playButtonScale = 0.5f;

        private Vector2 _fondoButtonPosition;
        private float _fondoButtonScale = 0.7f;

        private Texture2D _UziTexture;
        private Texture2D _platformTexture;
        private Texture2D _backgroundTexture;
        private Texture2D _centinelaTexture;
        private Texture2D _metalPlatformTexture;

        private Vector2 _UziPosition;
        private Vector2 _platformPosition;
        private Vector2 _centinelaPosition;
        private Vector2 _metalPlatformPosition;

        private float _gravity = 0.2f; // Ajustar si el personaje cae muy rápido
        private Vector2 _UziVelocity = Vector2.Zero; // Inicializar velocidad
        private bool _isJumping = false;

        private float _UziScale = 0.090f;
        private float _centinelaScale = 0.12f;
        private float _metalPlatformScale = 0.050f;
        private int _platformRepeatCount = 8;
        private int _backgroundRepeatCount;
        private SpriteFont _font;

        private Matrix _cameraTransform;
        private Vector2 _cameraPosition;

        private bool _gameStarted = false;
        private List<Vector2> _centinelaPositions;  // Posiciones de los centinelas
        private List<bool> _centinelaAlive;          // Si los centinelas están vivos
        private int _centinelaSpeed = 2;             // Velocidad de los centinelas
        private int _centinelaHits;                  // Número de hits que el centinela recibe
        private int _hitsToDefeat = 1;               // Número de saltos para matar a un centinela
        private int levelHeight;
        private Texture2D _flagTexture; // Textura de la bandera
        private Vector2 _flagPosition; // Posición de la bandera
        private float _flagScale = 0.2f; // Escala de la bandera
        private List<float> _centinelaTimers; // Controla cuándo aparece cada centinela
        private float _spawnDelay = 10f; // Tiempo en segundos entre cada aparición
        private int _maxCentinelas = 5;

        private bool _PgameWon = false; // Indicador de que el jugador ganó
        private bool _gameWon = false;  // Indica si el juego ha terminado con éxito
        private const int HitsToDefeat = 1; // Número de golpes necesarios para derrotar al centinela

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _platformPosition = new Vector2(0, 430); // Posición del piso/plataforma
            _UziPosition = new Vector2(100, _platformPosition.Y * _UziScale); // Posiciona a Mario sobre la plataforma
            _metalPlatformPosition = new Vector2(600, 200);

            // Inicializar los centinelas
            _centinelaPositions = new List<Vector2>();
            _centinelaAlive = new List<bool>();

            _centinelaPositions = new List<Vector2>
            {
            new Vector2(1380, _platformPosition.Y - 110),  // Centinela inicial en la pantalla
            new Vector2(1600, _platformPosition.Y - 110),
            new Vector2(1800, _platformPosition.Y - 110),
            new Vector2(2000, _platformPosition.Y - 110),
            new Vector2(2200, _platformPosition.Y - 110)
            };

            _centinelaAlive = new List<bool> { true, true, true, true, true };

            _centinelaTimers = new List<float>();
            for (int i = 0; i < _maxCentinelas; i++)
            {
                _centinelaTimers.Add(i * _spawnDelay); // Cada centinela tiene un temporizador escalonado
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Cargar texturas
            _playButtonTexture = Content.Load<Texture2D>("play_button");
            _fondoButtonTexture = Content.Load<Texture2D>("fondo_button");
            _UziTexture = Content.Load<Texture2D>("mario");
            _platformTexture = Content.Load<Texture2D>("plataforma");
            _backgroundTexture = Content.Load<Texture2D>("fondo");
            _centinelaTexture = Content.Load<Texture2D>("centinela");
            _metalPlatformTexture = Content.Load<Texture2D>("metal");
            _flagTexture = Content.Load<Texture2D>("flag"); // Carga la textura de la bandera
            _font = Content.Load<SpriteFont>("Calibri");


            _backgroundRepeatCount = (_platformTexture.Width * _platformRepeatCount) / _backgroundTexture.Width + 1;

            // Configurar la posición de la bandera al final del nivel
            float levelWidth = _platformTexture.Width * _platformRepeatCount;
            _flagPosition = new Vector2(
            _platformPosition.X + (_platformTexture.Width * (_platformRepeatCount - 1)) - _flagTexture.Width * _flagScale, // Coloca la bandera al final del piso
            _platformPosition.Y - _flagTexture.Height * _flagScale // Asegura que esté en el piso
);

            // Centralizar botones
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
                HandleMenuInput();
                return;
            }

            UpdateGameplay(gameTime, Get_centinelaPositions());

            if (_gameWon)
                return; // Si el juego ya terminó, no actualizamos nada más

            if (!_gameStarted)
            {
                HandleMenuInput();
                return;
            }

            UpdateGameplay(gameTime, Get_centinelaPositions());

            base.Update(gameTime);
        }

        private void HandleMenuInput()
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Point mousePosition = Mouse.GetState().Position;
                if (_playButtonBounds.Contains(mousePosition))
                {
                    _gameStarted = true;
                }
            }
        }

        private List<Vector2> Get_centinelaPositions()
        {
            return _centinelaPositions;
        }

        private void UpdateGameplay(GameTime gameTime, List<Vector2> _centinelaPositions)
        {
            // Obtener el estado del teclado
            var keyboardState = Keyboard.GetState();

            // Movimiento del personaje
            if (keyboardState.IsKeyDown(Keys.Right))
                _UziPosition.X += 3;
            if (keyboardState.IsKeyDown(Keys.Left))
                _UziPosition.X -= 3;

            // Salto del personaje
            if (keyboardState.IsKeyDown(Keys.Space) && !_isJumping)
            {
                _UziVelocity.Y = -15;
                _isJumping = true;
            }

            // Aplicar gravedad y mover al personaje
            _UziVelocity.Y += _gravity; // Aumenta la velocidad hacia abajo
            _UziPosition += _UziVelocity; // Aplica el movimiento vertical
            CheckCollisions();

            // Mover los centinelas
            for (int i = 0; i < _centinelaPositions.Count; i++)
            {
                // Actualizar temporizador del centinela
                if (_centinelaTimers[i] > 0)
                {
                    _centinelaTimers[i] -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    continue; // Si el temporizador no ha llegado a 0, el centinela no se mueve
                }

                // Mover el centinela
                _centinelaPositions[i] = new Vector2(
                    _centinelaPositions[i].X - _centinelaSpeed,
                    _centinelaPositions[i].Y
                );

                // Si el centinela sale de la pantalla, lo reinicia
                if (_centinelaPositions[i].X < -_centinelaTexture.Width * _centinelaScale)
                {
                    _centinelaPositions[i] = new Vector2(
                        _graphics.PreferredBackBufferWidth,
                        _platformPosition.Y - _centinelaTexture.Height * _centinelaScale
                    );
                    _centinelaTimers[i] = _spawnDelay; // Reinicia su temporizador
                }

                // Verificar colisión con el personaje
                if (_centinelaAlive[i] &&
                    CheckCollision(_UziPosition, _UziTexture, _centinelaPositions[i], _centinelaTexture))
                {
                    if (_UziVelocity.Y > 0) // Si el personaje está cayendo
                    {
                        _centinelaAlive[i] = false; // Centinela eliminado
                        _UziVelocity.Y = -4; // Rebote
                    }
                    else // Si no está cayendo, termina el juego
                    {
                        RestartGame();
                    }
                }
            }

            // Verificar si el personaje toca la bandera
            if (CheckCollision(_UziPosition, _UziTexture, _flagPosition, _flagTexture))
            {
                _gameWon = true;
            }

            UpdateCamera();
        }

        private void CheckCollisions()
            {
            if (_UziPosition.Y + _UziTexture.Height * _UziScale >= _platformPosition.Y &&
                _UziPosition.X + _UziTexture.Width * _UziScale >= _platformPosition.X &&
                _UziPosition.X <= _platformPosition.X + _platformTexture.Width * _platformRepeatCount)
            {
                _UziVelocity.Y = 0; // Detener la caída
                _isJumping = false;   // Permitir saltos de nuevo
                _UziPosition.Y = _platformPosition.Y - _UziTexture.Height * _UziScale; // Ajustar posición sobre la plataforma
            }
            else
            {
                _UziVelocity.Y += _gravity; // Continuar cayendo si no hay colisión
            }

            if (_UziPosition.Y + _UziTexture.Height * _UziScale >= _metalPlatformPosition.Y &&
                _UziPosition.Y < _metalPlatformPosition.Y + _metalPlatformTexture.Height * _metalPlatformScale &&
                _UziPosition.X + _UziTexture.Width * _UziScale >= _metalPlatformPosition.X &&
                _UziPosition.X <= _metalPlatformPosition.X + _metalPlatformTexture.Width * _metalPlatformScale)
            {
                if (_UziVelocity.Y > 0) // Solo si el personaje está cayendo
                {
                    _UziVelocity.Y = 0;
                    _isJumping = false;
                    _UziPosition.Y = _metalPlatformPosition.Y - _UziTexture.Height * _UziScale;
                }
            }
        }

                private void UpdateCamera()
                {
            _cameraPosition = new Vector2(_UziPosition.X - _graphics.PreferredBackBufferWidth / 2, 0);
            float levelWidth = _platformTexture.Width * _platformRepeatCount;
            _cameraPosition.X = Math.Max(0, Math.Min(_cameraPosition.X, levelWidth - _graphics.PreferredBackBufferWidth));
            _cameraTransform = Matrix.CreateTranslation(-_cameraPosition.X, -_cameraPosition.Y, 0);
                }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            if (!_gameStarted)
            {
                DrawMenu();
            }
            else
            {
                _spriteBatch.End();
                _spriteBatch.Begin(transformMatrix: _cameraTransform);
                DrawGameplay();
            }
            _spriteBatch.DrawString(_font, "Matar a los centinelas", new Vector2(10, 10), Color.White); // Esquina superior izquierda
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawMenu()
        {
            _spriteBatch.Draw(_fondoButtonTexture, _fondoButtonPosition, null, Color.White, 0f, Vector2.Zero, _fondoButtonScale, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_playButtonTexture, _playButtonPosition, null, Color.White, 0f, Vector2.Zero, _playButtonScale, SpriteEffects.None, 0f);
        }

        private void DrawGameplay()
        {
            // Dibujar el fondo
            for (int i = 0; i < _backgroundRepeatCount; i++)
            {
                _spriteBatch.Draw(_backgroundTexture, new Vector2(i * _backgroundTexture.Width, 0), Color.White);
            }

            // Dibujar las plataformas
            for (int i = 0; i < _platformRepeatCount; i++)
            {
                _spriteBatch.Draw(_platformTexture, new Vector2(_platformPosition.X + i * _platformTexture.Width, _platformPosition.Y), Color.White);
            }

            _spriteBatch.Draw(_metalPlatformTexture, _metalPlatformPosition, null, Color.White, 0f, Vector2.Zero, _metalPlatformScale, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_UziTexture, _UziPosition, null, Color.White, 0f, Vector2.Zero, _UziScale, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_centinelaTexture, _centinelaPosition, null, Color.White, 0f, Vector2.Zero, _centinelaScale, SpriteEffects.None, 0f);

            // Dibujar los centinelas solo si están vivos
            for (int i = 0; i < _centinelaPositions.Count; i++)
            {
                if (_centinelaAlive[i])
                {
                    _spriteBatch.Draw(_centinelaTexture, _centinelaPositions[i], null, Color.White, 0f, Vector2.Zero, _centinelaScale, SpriteEffects.None, 0f);
                }
            }

            // Dibujar al personaje
            _spriteBatch.Draw(_UziTexture, _UziPosition, null, Color.White, 0f, Vector2.Zero, _UziScale, SpriteEffects.None, 0f);

            // Dibujar la bandera
            _spriteBatch.Draw(_flagTexture, _flagPosition, null, Color.White, 0f, Vector2.Zero, _flagScale, SpriteEffects.None, 0f);

            // Mostrar mensaje de victoria si el juego ha terminado
            if (_gameWon)
            {
                _spriteBatch.End();
                _spriteBatch.Begin();
                string victoryMessage = "Ganaste";
                var font = Content.Load<SpriteFont>("Calibri"); // Necesitas un SpriteFont cargado
                var messageSize = font.MeasureString(victoryMessage);
                var screenCenter = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
                _spriteBatch.DrawString(font, victoryMessage, screenCenter - messageSize / 2, Color.Yellow);
            }
        }

        private bool CheckCollision(Vector2 position1, Texture2D texture1, Vector2 position2, Texture2D texture2)
        {
            Rectangle rect1 = new Rectangle((int)position1.X, (int)position1.Y, (int)(texture1.Width * _UziScale), (int)(texture1.Height * _UziScale));
            Rectangle rect2 = new Rectangle((int)position2.X, (int)position2.Y, (int)(texture2.Width * _flagScale), (int)(texture2.Height * _flagScale));
            return rect1.Intersects(rect2);
        }
        private void RestartGame()
        {
            // Reiniciar la posición de Mario
            _UziPosition = new Vector2(100, _platformPosition.Y - _UziScale * 100);

            // Reiniciar la posición del centinela
            _centinelaPosition.X = 1380;

            // Reiniciar la velocidad de Mario
            _UziVelocity = Vector2.Zero;

            // Resetear el estado de salto
            _isJumping = false;
        }
    }
}
