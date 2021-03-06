using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

/// <summary>
/// The game object.
/// </summary>
public class Game
{
  #region Properties

  /// <summary>
  /// Currently active chemicals.
  /// </summary>
  public IEnumerable<IChemical> ActiveChemicals => _activeChemicals;
  private List<IChemical> _activeChemicals;

  /// <summary>
  /// Currently unlocked chemicals.
  /// </summary>
  public IEnumerable<string> UnlockedChemicals => _unlockedChemicals;
  private List<string> _unlockedChemicals;

  #endregion Properties

  #region Construction

  /// <summary>
  /// Creates a game from the given <paramref name="data"/>.
  /// </summary>
  /// <param name="data">Data to create game from.</param>
  /// <returns>Created game.</returns>
  public static Game FromGameData(GameSaveData data)
  {
    var game = new Game
    {
      _activeChemicals = data.ActiveElements.Select(e => (IChemical)new Chemical(e)).ToList(),
      _unlockedChemicals = data.UnlockedElements
    };

    return game;
  }

  /// <summary>
  /// Creates a new game.
  /// </summary>
  /// <returns>New game.</returns>
  public static Game New()
  {
    var game = new Game();
    game.AddElement(new Chemical("Fire"));
    game.AddElement(new Chemical("Water"));
    return game;
  }

  /// <summary>
  /// Constructor.
  /// </summary>
  private Game()
  {
    _activeChemicals = new List<IChemical>();
    _unlockedChemicals = new List<string>();
  }

  #endregion Construction

  /// <summary>
  /// Combines the given <paramref name="chemicals"/>.
  /// </summary>
  /// <param name="chemicals">Chemicals to combine.</param>
  public IEnumerable<IChemical> Combine(IEnumerable<IChemical> chemicals)
  {
    if (chemicals == null || chemicals.Any(e => e == null))
      throw new ArgumentNullException(nameof(chemicals));
    else
    {
      var newElements = chemicals.First().CombineWith(chemicals.Skip(chemicals.Count() - 1));
      if (newElements.Count() != 0)
      {
        // remove old elements
        foreach (var e in chemicals)
          _activeChemicals.Remove(e);

        // add new elements
        foreach (var element in newElements)
          AddElement(element);
      }

      return newElements;
    }
  }

  /// <summary>
  /// Combines the given <paramref name="chemical1"/> with <paramref name="chemical2"/>.
  /// </summary>
  /// <param name="chemical1">Chemical to combine with <paramref name="chemical2"/>.</param>
  /// <param name="chemical2">Chemical to combine with <paramref name="chemical1"/>.</param>
  public IEnumerable<IChemical> Combine(IChemical chemical1, IChemical chemical2)
  {
    return Combine(new[] { chemical1, chemical2 });
  }

  /// <summary>
  /// Disassembles the given <paramref name="chemical"/> into its sub parts.
  /// </summary>
  /// <param name="chemical">Chemical to disassemble.</param>
  public IEnumerable<IChemical> Disassemble(IChemical chemical)
  {
    if (chemical == null)
      throw new ArgumentNullException(nameof(chemical));
    else if (!ActiveChemicals.Contains(chemical))
      throw new ArgumentException(nameof(chemical));
    else
    {
      var newElements = chemical.Disassemble();
      if (newElements.Count() > 0)
      {
        // remove old element
        _activeChemicals.Remove(chemical);

        // add new elements
        foreach (var e in newElements)
          AddElement(e);
      }

      return newElements;
    }
  }

  /// <summary>
  /// Adds a chemical with the given <paramref name="name"/>
  /// to the <see cref="ActiveChemicals"/>.
  /// </summary>
  /// <param name="name"></param>
  public IChemical AddChemicalToWorkspace(string name)
  {
    if (!UnlockedChemicals.Contains(name))
      throw new ArgumentException($"{name} is not unlocked", nameof(name));

    var chemical = new Chemical(name);
    AddElement(chemical);
    return chemical;
  }

  /// <summary>
  /// Removes all active chemicals.
  /// </summary>
  public void CleanUp()
  {
    _activeChemicals.RemoveAll(e => true);
  }

  /// <summary>
  /// Adds (and unlocks) the given <paramref name="chemical"/>.
  /// </summary>
  /// <param name="chemical">Chemical to add.</param>
  private void AddElement(IChemical chemical)
  {
    if (chemical == null)
      throw new ArgumentNullException(nameof(chemical));

    _activeChemicals.Add(chemical);
    if (!UnlockedChemicals.Contains(chemical.Name))
      _unlockedChemicals.Add(chemical.Name);
  }
}