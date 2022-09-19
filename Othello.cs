#nullable enable
using System;
using static System.Console;

namespace Bme121
{
    record Player( string Colour, string Symbol, string Name );
    
    // The `record` is a kind of automatic class in the sense that the compiler generates
    // the fields and constructor and some other things just from this one line.
    // There's a rationale for the capital letters on the variable names (later).
    // For now you can think of it as roughly equivalent to a nonstatic `class` with three
    // public fields and a three-parameter constructor which initializes the fields.
    // It would be like the following. The 'readonly' means you can't change the field value
    // after it is set by the constructor method.
    
    //class Player
    //{
        //public readonly string Colour;
        //public readonly string Symbol;
        //public readonly string Name;
        
        //public Player( string Colour, string Symbol, string Name )
        //{
            //this.Colour = Colour;
            //this.Symbol = Symbol;
            //this.Name = Name;
        //}
    //}
    
    static partial class Program
    {
        // Display common text for the top of the screen.
        
        static void Welcome( )
        {
			WriteLine ("Welcome to Othello");
        }
        
        // Collect a player name or default to form the player record.
        
        static Player NewPlayer( string colour, string symbol, string defaultName )
        {
			Write ( "Player({1}) please enter your name or click <enter> for default name {0}: ", defaultName, symbol);
			string playerName = ReadLine();
			if (playerName.Length == 0) 
			{
				playerName = defaultName;
			}
			WriteLine($"{colour} player's name: {playerName}");
			Write ("Click <enter> to initialize your symbol {0}: ", symbol);
			string playerSymbol = ReadLine();
			if (playerSymbol.Length == 0) 
			{
				playerSymbol = symbol;
			}

            return new Player(colour,playerSymbol,playerName );
        }
        
        // Determine which player goes first or default.
        
        static int GetFirstTurn( Player[ ] players, int defaultFirst )
        {
			while (true)
			{
				WriteLine ("Who would like to go first? Type player name or click enter for default");
				string response = ReadLine();
				if (response.Length == 0) 
				{
					WriteLine($"{players[defaultFirst].Name} is going first");
					return defaultFirst;
				}
				if (response == players[0].Name)
				{
					WriteLine($"{players[0].Name} is going first");
					return 0;
				}
				else if (response == players[1].Name)
				{
					WriteLine($"{players[1].Name} is going first");
					return 1;
				}
				else 
				{
					WriteLine("Error, re enter the name");
				}
			}
			
        }
        
        // Get a board size (between 4 and 26 and even) or default, for one direction.
        
        static int GetBoardSize( string direction, int defaultSize )
        {
			while (true)
			{
				Write ("Enter board {0}: (even and between 4 to 26) or <Enter> for {1}: ", direction,defaultSize);
				string answer = ReadLine();
				if (answer.Length == 0)
				{
					return defaultSize;
				}
				int size = int.Parse(answer);
				if ( size >=4 && size%2 == 0 && size <= 26)
				{
					return size;
				}
				else
				{
					WriteLine( "Invalid size, please enter another size");
				}
			}
        }
        
        // Get a move from a player.
        
        static string GetMove( Player player )
        {
			WriteLine( "Options: quit, skip, and two letters (row and column) to make a move ex. 'aa'");
			Write ($"{player.Name} ({player.Symbol}) enter your move: ");
            return ReadLine();
        }
        
        // Try to make a move. Return true if it worked.
        
        static bool TryMove( string[ , ] board, Player player, string move )
        {
			string rowLetter = move.Substring (0,1);
			string colLetter = move.Substring (1,1);
			int rowIndex = IndexAtLetter(rowLetter);
			int colIndex = IndexAtLetter(colLetter);
			
            if (move == "skip") return true;
            
			if (move.Length != 2) 
			{
				return false;
			}
			else
			{
				if (rowIndex < 0 | rowIndex > board.GetLength( 0 ) -1 ) return false;
				else if (colIndex < 0 | colIndex > board.GetLength(1) -1 ) return false;
				else if ( board[ rowIndex, colIndex] != " ")return false;
				//board[rowIndex,colIndex] = player.Symbol;
			}
			
			bool [] passedDirections = new bool [8];
			
			passedDirections[0] = TryDirection(board,player,rowIndex,-1,colIndex,-1); //top left
			passedDirections[1] = TryDirection(board,player,rowIndex,-1,colIndex,0); //top 
			passedDirections[2] = TryDirection(board,player,rowIndex,-1,colIndex,1); //top right
			passedDirections[3] = TryDirection(board,player,rowIndex,0,colIndex,1) ;//right
			passedDirections[4] = TryDirection(board,player,rowIndex,1,colIndex,1) ;//bottom right
			passedDirections[5] = TryDirection(board,player,rowIndex,1,colIndex,0) ;//bottom
			passedDirections[6] = TryDirection(board,player,rowIndex,1,colIndex,-1) ;//bottom left
			passedDirections[7] = TryDirection(board,player,rowIndex,0,colIndex,-1); //left
			
			foreach (bool direction in passedDirections)
			{
				if (direction == true)
				{
					return true;
				}
			}
			return false;
        }
        
        // Do the flips along a direction specified by the row and column delta for one step.
        
        static bool TryDirection( string[ , ] board, Player player,
            int moveRow, int deltaRow, int moveCol, int deltaCol )
        {
			int nextRow = moveRow + deltaRow;
			int nextCol = moveCol + deltaCol;
			int flipCounter = 0;
			
			
			if(nextRow<0 || nextRow >= board.GetLength(0)) return false;
			if(nextCol<0 || nextCol >= board.GetLength(1)) return false;
			
			if ( board[nextRow,nextCol]== player.Symbol) return false;
			if ( board[nextRow,nextCol]== " ") return false;
			
			while ( true )
			{
				if ( board[nextRow,nextCol] == " ") return false;
				if ( board [nextRow,nextCol] != player.Symbol )
				{ 
					nextRow = nextRow + deltaRow;
					nextCol = nextCol + deltaCol;
					flipCounter++;
					if(nextRow<0 || nextRow >= board.GetLength(0)) return false;
					if(nextCol<0 || nextCol >= board.GetLength(1)) return false;
				}
				if ( board [nextRow,nextCol] == player.Symbol)
				{
					nextRow = moveRow ;
					nextCol = moveCol ;
					while ( flipCounter >= 0 )
					{
						board[nextRow,nextCol] = player.Symbol;
						nextRow = nextRow + deltaRow;
						nextCol = nextCol +deltaCol;
						flipCounter--;
					}
					return true;
				}
			}
        }
        
        // Count the discs to find the score for a player.
        
        static int GetScore( string[ , ] board, Player player )
        {
			int score = 0;
			for (int r = 0; r<board.GetLength(0); r++)
			{
				for(int c = 0; c<board.GetLength(1); c++)
				{
					if (board[r,c]== player.Symbol) score++;
				}
			}

            return score;
        }
        
        // Display a line of scores for all players.
        
        static void DisplayScores( string[ , ] board, Player[ ] players )
        {
			for (int i=0; i<players.Length ; i++)
			{
				int score = GetScore(board,players[i]);
				WriteLine($" {players[i].Name} ({players[i].Symbol}): {score}");
			}
        }
        
        // Display winner(s) and categorize their win over the defeated player(s).
        
        static void DisplayWinners( string[ , ] board, Player[ ] players )
        {
			int winningScore= 0;
			string winner = " ";
			
			for ( int i = 0 ; i<players.Length ; i++)
			{
				if (GetScore(board,players[i]) > winningScore)
				{
					winningScore = GetScore(board,players[i]);
					winner = players[i].Name;
				}
			}
			Write ( "{0} wins: ", winner);
			
			int highestScore = winningScore;
			
			for(int i=0; i<players.Length ; i++)
			{
				string comparedPlayerName= players[i].Name;
				int difference = highestScore- GetScore(board,players[i]);
				
				if (difference >=2 && difference <=10 )
				{
					Write( $" It was a close game against {comparedPlayerName}. Won by {difference}.");
				}
				else if (difference >=12 && difference <=24 )
				{
					Write( $" It was a hot game against {comparedPlayerName}. Won by {difference}.");
				}
				else if (difference >=26 && difference <=38 )
				{
					Write( $" It was a fight game against {comparedPlayerName}. Won by {difference}.");
				}
				else if (difference >=40 && difference <=52 )
				{
					Write( $" It was a walkaway game against {comparedPlayerName}. Won by {difference}.");
				}
				else if (difference >=54 && difference <=64 )
				{
					Write( $" It was a perfect game against {comparedPlayerName}. Won by {difference}.");
				}
			}
			
        }
        static bool ExistsPossibleMoves ( string [,] board, Player player)
        {
			for ( int r = 0 ; r < board.GetLength(0) ; r++)
			{
				for ( int c = 0 ; c < board.GetLength(1) ; c++)
				{
					if ( board[r,c] == " " )
					{
						if ( CheckMove(board, player, r, c ))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		static bool CheckMove( string[ , ] board, Player player, int rowIndex, int colIndex )
        {			
			bool [] passedDirections = new bool [8];
			
			passedDirections[0] = CheckDirection(board,player,rowIndex,-1,colIndex,-1); //top left
			passedDirections[1] = CheckDirection(board,player,rowIndex,-1,colIndex,0); //top 
			passedDirections[2] = CheckDirection(board,player,rowIndex,-1,colIndex,1); //top right
			passedDirections[3] = CheckDirection(board,player,rowIndex,0,colIndex,1) ;//right
			passedDirections[4] = CheckDirection(board,player,rowIndex,1,colIndex,1) ;//bottom right
			passedDirections[5] = CheckDirection(board,player,rowIndex,1,colIndex,0) ;//bottom
			passedDirections[6] = CheckDirection(board,player,rowIndex,1,colIndex,-1) ;//bottom left
			passedDirections[7] = CheckDirection(board,player,rowIndex,0,colIndex,-1); //left
			
			foreach (bool direction in passedDirections)
			{
				if (direction == true)
				{
					return true;
				}
			}
			return false;
        }
		
		static bool CheckDirection( string[ , ] board, Player player,
        int moveRow, int deltaRow, int moveCol, int deltaCol )
        {
			int nextRow = moveRow + deltaRow;
			int nextCol = moveCol + deltaCol;
			int flipCounter = 0;
			
			
			if(nextRow<0 || nextRow >= board.GetLength(0)) return false;
			if(nextCol<0 || nextCol >= board.GetLength(1)) return false;
			
			if ( board[nextRow,nextCol]== player.Symbol) return false;
			if ( board[nextRow,nextCol]== " ") return false;
			
			while ( true )
			{
				if(nextRow<0 || nextRow >= board.GetLength(0)) return false;
				if(nextCol<0 || nextCol >= board.GetLength(1)) return false;
				
				if ( board[nextRow,nextCol] == " ") return false;
				if ( board [nextRow,nextCol] != player.Symbol )
				{ 
					nextRow = nextRow + deltaRow;
					nextCol = nextCol + deltaCol;

					if(nextRow<0 || nextRow >= board.GetLength(0)) return false;
					if(nextCol<0 || nextCol >= board.GetLength(1)) return false;
				}
				if ( board [nextRow,nextCol] == player.Symbol) return true;
			}
        }
        static void Main( )
        {
            // Set up the players and game.
            // Note: I used an array of 'Player' objects to hold information about the players.
            // This allowed me to just pass one 'Player' object to methods needing to use
            // the player name, colour, or symbol in 'WriteLine' messages or board operation.
            // The array aspect allowed me to use the index to keep track or whose turn it is.
            // It is also possible to use separate variables or separate arrays instead
            // of an array of objects. It is possible to put the player information in
            // global variables (static field variables of the 'Program' class) so they
            // can be accessed by any of the methods without passing them directly as arguments.
            
            Welcome( );
            
            Player[ ] players = new Player[ ] 
            {
                NewPlayer( colour: "black", symbol: "X", defaultName: "Black" ),
                NewPlayer( colour: "white", symbol: "O", defaultName: "White" ),
            };
            
            int turn = GetFirstTurn( players, defaultFirst: 0 );
           
            int rows = GetBoardSize( direction: "rows",    defaultSize: 8 );
            int cols = GetBoardSize( direction: "columns", defaultSize: 8 );
            
            string[ , ] game = NewBoard( rows, cols );
            
            // Play the game.
            
            bool gameOver = false;
            while( ! gameOver )
            {
                Welcome( );
                DisplayBoard( game ); 
                DisplayScores( game, players );
                string move = GetMove( players[ turn ] );
                if( move == "quit" ) gameOver = true;
                else
                {	
					
                    bool madeMove = TryMove( game, players[ turn ], move );
                    if( madeMove ) turn = ( turn + 1 ) % players.Length;
                    else 
                    {
                        Write( " Your choice didn't work!" );
                        Write( " Press <Enter> to try again." );
                        ReadLine( ); 
                    }
                }
                if (!ExistsPossibleMoves(game,players[turn]) && !ExistsPossibleMoves(game, players[turn +1])) gameOver = true;
            }
            
            // Show fhe final results.
            DisplayBoard( game );
            DisplayScores( game, players ); 
            DisplayWinners( game, players );
            WriteLine( );
        }
    }
}
