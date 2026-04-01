"""Initial Pong game scaffold using Turtle graphics."""
import time
import turtle

SCREEN_WIDTH = 800
SCREEN_HEIGHT = 600
PADDLE_SPEED = 12
BALL_SPEED_X = 5
BALL_SPEED_Y = 5
FRAME_DELAY = 0.01
BALL_RADIUS = 10
PADDLE_HALF_HEIGHT = 50
PADDLE_HALF_WIDTH = 10
SCORE_FONT = ("Courier", 18, "normal")


class Paddle(turtle.Turtle):
    def __init__(self, position):
        super().__init__(shape="square")
        self.speed(0)
        self.color("white")
        self.shapesize(stretch_wid=5, stretch_len=1)
        self.penup()
        self.goto(position)
        self._direction = 0  # -1 = down, 1 = up

    def start_move_up(self):
        self._direction = 1

    def start_move_down(self):
        self._direction = -1

    def stop_move_up(self):
        if self._direction == 1:
            self._direction = 0

    def stop_move_down(self):
        if self._direction == -1:
            self._direction = 0

    def update(self):
        if self._direction == 0:
            return

        new_y = self.ycor() + self._direction * PADDLE_SPEED
        upper_bound = SCREEN_HEIGHT // 2 - 50
        lower_bound = -upper_bound
        new_y = max(min(new_y, upper_bound), lower_bound)
        self.sety(new_y)


class Ball(turtle.Turtle):
    def __init__(self):
        super().__init__(shape="circle")
        self.speed(0)
        self.color("white")
        self.penup()
        self.goto(0, 0)
        self.dx = BALL_SPEED_X
        self.dy = BALL_SPEED_Y

    def update(self):
        self.setx(self.xcor() + self.dx)
        self.sety(self.ycor() + self.dy)

        # Bounce on top/bottom walls
        if self.ycor() > SCREEN_HEIGHT // 2 - BALL_RADIUS:
            self.sety(SCREEN_HEIGHT // 2 - BALL_RADIUS)
            self.dy *= -1
        if self.ycor() < -SCREEN_HEIGHT // 2 + BALL_RADIUS:
            self.sety(-SCREEN_HEIGHT // 2 + BALL_RADIUS)
            self.dy *= -1

    def reset(self):
        """Return ball to center and send it toward last scorer."""
        self.goto(0, 0)
        self.dx = (-1 if self.dx > 0 else 1) * BALL_SPEED_X
        self.dy = (1 if self.dy > 0 else -1) * BALL_SPEED_Y


class Scoreboard(turtle.Turtle):
    def __init__(self):
        super().__init__(visible=False)
        self.speed(0)
        self.color("white")
        self.penup()
        self.goto(0, SCREEN_HEIGHT // 2 - 40)
        self.left_score = 0
        self.right_score = 0
        self._draw()

    def _draw(self):
        self.clear()
        self.write(
            f"Player A: {self.left_score}  Player B: {self.right_score}",
            align="center",
            font=SCORE_FONT,
        )

    def point_left(self):
        self.left_score += 1
        self._draw()

    def point_right(self):
        self.right_score += 1
        self._draw()


screen = turtle.Screen()
screen.title("Pong")
screen.bgcolor("black")
screen.setup(width=SCREEN_WIDTH, height=SCREEN_HEIGHT)
screen.tracer(0)

left_paddle = Paddle(position=(-SCREEN_WIDTH // 2 + 50, 0))
right_paddle = Paddle(position=(SCREEN_WIDTH // 2 - 50, 0))
ball = Ball()
scoreboard = Scoreboard()

screen.listen()
screen.onkeypress(left_paddle.start_move_up, "w")
screen.onkeypress(left_paddle.start_move_down, "s")
screen.onkeyrelease(left_paddle.stop_move_up, "w")
screen.onkeyrelease(left_paddle.stop_move_down, "s")

screen.onkeypress(right_paddle.start_move_up, "Up")
screen.onkeypress(right_paddle.start_move_down, "Down")
screen.onkeyrelease(right_paddle.stop_move_up, "Up")
screen.onkeyrelease(right_paddle.stop_move_down, "Down")


while True:
    screen.update()
    left_paddle.update()
    right_paddle.update()
    ball.update()

    # Paddle collisions keep the ball within playfield bounds
    if ball.dx > 0 and ball.xcor() >= right_paddle.xcor() - (PADDLE_HALF_WIDTH + BALL_RADIUS):
        if abs(ball.ycor() - right_paddle.ycor()) <= PADDLE_HALF_HEIGHT:
            ball.setx(right_paddle.xcor() - (PADDLE_HALF_WIDTH + BALL_RADIUS))
            ball.dx *= -1

    if ball.dx < 0 and ball.xcor() <= left_paddle.xcor() + (PADDLE_HALF_WIDTH + BALL_RADIUS):
        if abs(ball.ycor() - left_paddle.ycor()) <= PADDLE_HALF_HEIGHT:
            ball.setx(left_paddle.xcor() + (PADDLE_HALF_WIDTH + BALL_RADIUS))
            ball.dx *= -1

    # Scoring when ball exits horizontal bounds
    if ball.xcor() > SCREEN_WIDTH // 2 - BALL_RADIUS:
        scoreboard.point_left()
        ball.reset()
    elif ball.xcor() < -SCREEN_WIDTH // 2 + BALL_RADIUS:
        scoreboard.point_right()
        ball.reset()

    time.sleep(FRAME_DELAY)

