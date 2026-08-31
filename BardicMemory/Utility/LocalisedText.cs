namespace BardicMemory.Utility;

/// <summary>
/// Every phrase the plugin shows the player, written out in each language.
/// </summary>
public static class LocalisedText
{
    public static LocalisedString PlayTab { get; } = new("Play", "プレイ", "Spielen", "Jouer");

    public static LocalisedString HistoryTab { get; } = new("History", "履歴", "Verlauf", "Historique");

    public static LocalisedString SettingsTab { get; } = new("Settings", "設定", "Einstellungen", "Paramètres");

    public static LocalisedString SupportTab { get; } = new("Support", "サポート", "Support", "Assistance");

    public static LocalisedString Submit { get; } = new("Submit", "回答する", "Bestätigen", "Valider");

    public static LocalisedString IncreaseDuration { get; } = new(
        "Increase Duration",
        "再生時間を延ばす",
        "Dauer verlängern",
        "Allonger l'extrait");

    public static LocalisedString GiveUp { get; } = new("Give up", "あきらめる", "Aufgeben", "Abandonner");

    public static LocalisedString StartGame { get; } = new(
        "Start a Game",
        "ゲームを開始",
        "Spiel starten",
        "Commencer une partie");

    public static LocalisedString EndGame { get; } = new(
        "End Game",
        "ゲームを終了",
        "Spiel beenden",
        "Terminer la partie");

    public static LocalisedString NoSongs { get; } = new(
        "No songs could be read from the game data.",
        "ゲームデータから曲を読み込めませんでした。",
        "Aus den Spieldaten konnten keine Stücke gelesen werden.",
        "Aucun morceau n'a pu être lu depuis les données du jeu.");

    public static LocalisedString SceneTakenOver { get; } = new(
        "Another plugin keeps replacing the music. Turn off Orchestrion's song replacement to play.",
        "他のプラグインが音楽を上書きしています。Orchestrionの曲置換を解除してください。",
        "Ein anderes Plugin ersetzt die Musik laufend. Schalte Orchestrions Titelersetzung aus.",
        "Un autre plugin remplace la musique. Désactivez le remplacement de morceaux d'Orchestrion.");

    public static LocalisedString OrchestrionInTheWay { get; } = new(
        "An orchestrion is playing here, so clips cannot be heard. Switch it off to play.",
        "オーケストリオンの再生中は曲を聞けません。停止してからお試しください。",
        "Hier spielt ein Orchestrion, daher sind keine Ausschnitte zu hören. Schalte es aus, um zu spielen.",
        "Un orchestrion joue ici, les extraits sont donc inaudibles. Éteignez-le pour jouer.");

    public static LocalisedString SongCount { get; } = new("{0} songs", "{0}曲", "{0} Stücke", "{0} morceaux");

    public static LocalisedString Skipped { get; } = new("Skipped", "スキップ", "Übersprungen", "Passé");

    public static LocalisedString ClipVolume { get; } = new("Clip volume", "音量", "Lautstärke", "Volume");

    public static LocalisedString ClipVolumeReading { get; } = new(
        "Clip volume: {0}%",
        "音量: {0}%",
        "Lautstärke: {0} %",
        "Volume : {0} %");

    public static LocalisedString ResetVolume { get; } = new(
        "Reset to full",
        "最大に戻す",
        "Auf Maximum zurücksetzen",
        "Remettre au maximum");

    public static LocalisedString SearchSong { get; } = new(
        "Search a song",
        "曲を検索",
        "Stück suchen",
        "Rechercher un morceau");

    public static LocalisedString SolvedAt { get; } = new(
        "Solved at {0}",
        "{0}で正解",
        "Gelöst bei {0}",
        "Trouvé à {0}");

    public static LocalisedString PlaysIn { get; } = new(
        "Plays in {0}",
        "再生場所: {0}",
        "Zu hören in {0}",
        "Joué à {0}");

    public static LocalisedString PlayFullTrack { get; } = new(
        "Play full track",
        "フルで再生",
        "Ganzes Stück abspielen",
        "Écouter en entier");

    public static LocalisedString NextSong { get; } = new(
        "Next song",
        "次の曲",
        "Nächstes Stück",
        "Morceau suivant");

    public static LocalisedString StatRounds { get; } = new("Rounds", "ラウンド", "Runden", "Manches");

    public static LocalisedString StatSolved { get; } = new("Solved", "正解", "Gelöst", "Trouvés");

    public static LocalisedString StatPoints { get; } = new("Points", "ポイント", "Punkte", "Points");

    public static LocalisedString StatAverage { get; } = new("Average", "平均", "Durchschnitt", "Moyenne");

    public static LocalisedString StatStreak { get; } = new("Streak", "連続正解", "Serie", "Série");

    public static LocalisedString NoRoundsYet { get; } = new(
        "No rounds played yet.",
        "まだプレイしたラウンドがありません。",
        "Noch keine Runden gespielt.",
        "Aucune manche jouée pour l'instant.");

    public static LocalisedString ColumnSong { get; } = new("Song", "曲", "Stück", "Morceau");

    public static LocalisedString ColumnScore { get; } = new("Score", "スコア", "Punkte", "Score");

    public static LocalisedString ColumnTime { get; } = new("Time", "日時", "Zeit", "Heure");

    public static LocalisedString EndedAt { get; } = new(
        "Ended at {0}",
        "{0}で終了",
        "Beendet bei {0}",
        "Terminé à {0}");

    public static LocalisedString SilenceMusicToggle { get; } = new(
        "Silence the game's music during a round",
        "ラウンド中はゲームの音楽を止める",
        "Spielmusik während einer Runde stummschalten",
        "Couper la musique du jeu pendant une manche");

    public static LocalisedString SilenceMusicHelp { get; } = new(
        "Holds the game's background music on the silent track while a round is running so only "
        + "the clip is audible, then hands it back when the round ends.",
        "ラウンド中はゲームのBGMを無音の曲に固定し、出題の音だけが聞こえるようにします。ラウンドが終わると元に戻します。",
        "Hält die Hintergrundmusik des Spiels während einer Runde auf dem stummen Titel, sodass nur "
        + "der Ausschnitt zu hören ist, und gibt sie am Ende der Runde zurück.",
        "Maintient la musique de fond du jeu sur la piste silencieuse pendant une manche afin de "
        + "n'entendre que l'extrait, puis la rétablit à la fin de la manche.");

    public static LocalisedString SkipSolvedToggle { get; } = new(
        "Skip songs already guessed correctly",
        "正解済みの曲を出題しない",
        "Bereits erratene Stücke überspringen",
        "Ignorer les morceaux déjà trouvés");

    public static LocalisedString SkipSolvedHelp { get; } = new(
        "Keeps solved songs out of the pool until every song in the category has been solved.",
        "そのカテゴリーの曲をすべて正解するまで、正解済みの曲は出題されません。",
        "Hält gelöste Stücke aus dem Pool heraus, bis jedes Stück der Kategorie gelöst wurde.",
        "Exclut les morceaux trouvés du tirage jusqu'à ce que tous les morceaux de la catégorie "
        + "aient été trouvés.");

    public static LocalisedString HistoryLimit { get; } = new(
        "Rounds kept in history",
        "履歴に残すラウンド数",
        "Im Verlauf gespeicherte Runden",
        "Manches conservées dans l'historique");

    public static LocalisedString ClearHistory { get; } = new(
        "Clear history and stats",
        "履歴と統計を消去",
        "Verlauf und Statistiken löschen",
        "Effacer l'historique et les statistiques");

    public static LocalisedString ClearHistoryWarning { get; } = new(
        "This wipes every recorded round and every total.",
        "記録したすべてのラウンドと集計が消えます。",
        "Damit werden alle aufgezeichneten Runden und alle Summen gelöscht.",
        "Cela supprime toutes les manches enregistrées et tous les totaux.");

    public static LocalisedString ClearConfirm { get; } = new("Clear it", "消去する", "Löschen", "Effacer");

    public static LocalisedString Cancel { get; } = new("Cancel", "キャンセル", "Abbrechen", "Annuler");

    public static LocalisedString DiscordButton { get; } = new(
        "Join the Discord",
        "Discordに参加",
        "Discord beitreten",
        "Rejoindre le Discord");

    public static LocalisedString FaqPlayQuestion { get; } = new(
        "How do I play?",
        "遊び方は？",
        "Wie wird gespielt?",
        "Comment jouer ?");

    public static LocalisedString FaqPlayAnswer { get; } = new(
        "Press play to hear the opening of a random track. Name it from the search box and you "
        + "score six points. Stretch the clip if you need longer and the points come down each "
        + "time you do.",
        "再生を押すと、ランダムな曲の冒頭が流れます。検索ボックスから曲名を当てれば6ポイントです。"
        + "もっと長く聴きたいときは再生時間を延ばせますが、延ばすたびにポイントは下がります。",
        "Drücke auf Wiedergabe, um den Anfang eines zufälligen Stücks zu hören. Nenne es über das "
        + "Suchfeld und du erhältst sechs Punkte. Verlängere den Ausschnitt, wenn du mehr Zeit "
        + "brauchst; mit jedem Mal sinkt die Punktzahl.",
        "Appuyez sur lecture pour entendre le début d'un morceau au hasard. Nommez-le depuis le "
        + "champ de recherche et vous marquez six points. Allongez l'extrait s'il vous faut plus "
        + "de temps : les points diminuent à chaque fois.");

    public static LocalisedString FaqSourceQuestion { get; } = new(
        "Where do the songs come from?",
        "曲はどこから来ているの？",
        "Woher stammen die Stücke?",
        "D'où viennent les morceaux ?");

    public static LocalisedString FaqSourceAnswer { get; } = new(
        "The game's own orchestrion library, read straight from the game files. You can search "
        + "for a track by its title in English, Japanese, German or French, whichever you know "
        + "it by.",
        "ゲーム内のオーケストリオン譜のライブラリーから、ゲームファイルを直接読み込んでいます。"
        + "曲名は英語・日本語・ドイツ語・フランス語のいずれでも検索できます。",
        "Aus der Orchestrion-Bibliothek des Spiels, direkt aus den Spieldateien gelesen. Du kannst "
        + "nach dem Titel auf Englisch, Japanisch, Deutsch oder Französisch suchen, je nachdem, "
        + "wie du ihn kennst.",
        "De la bibliothèque d'orchestrion du jeu, lue directement dans les fichiers du jeu. Vous "
        + "pouvez rechercher un morceau par son titre en anglais, japonais, allemand ou français, "
        + "selon celui que vous connaissez.");

    public static LocalisedString FaqQuietQuestion { get; } = new(
        "Why has my music gone quiet?",
        "音楽が止まるのはなぜ？",
        "Warum ist meine Musik verstummt?",
        "Pourquoi ma musique s'est-elle arrêtée ?");

    public static LocalisedString FaqQuietAnswer { get; } = new(
        "The game's background music is held on the silent track while a round is running so only "
        + "the clip is audible and handed back the moment the round ends. You can switch that off "
        + "on the settings tab.",
        "ラウンド中は出題の音だけが聞こえるように、ゲームのBGMを無音の曲に固定しています。"
        + "ラウンドが終わればすぐに元に戻ります。設定タブでオフにできます。",
        "Die Hintergrundmusik des Spiels wird während einer Runde auf dem stummen Titel gehalten, "
        + "damit nur der Ausschnitt zu hören ist, und sofort nach Rundenende zurückgegeben. Du "
        + "kannst das im Einstellungen-Tab abschalten.",
        "La musique de fond du jeu est maintenue sur la piste silencieuse pendant une manche afin "
        + "de n'entendre que l'extrait, puis rétablie dès la fin de la manche. Vous pouvez "
        + "désactiver cela dans l'onglet Paramètres.");

    public static LocalisedString FaqWrongGuessQuestion { get; } = new(
        "Wrong guesses do not seem to cost anything.",
        "不正解にペナルティはないの？",
        "Falsche Antworten scheinen nichts zu kosten.",
        "Les mauvaises réponses ne coûtent rien ?");

    public static LocalisedString FaqWrongGuessAnswer { get; } = new(
        "They do not. Guess as often as you like at whatever clip length you are on. Only "
        + "stretching the clip costs you points.",
        "ありません。今の再生時間のまま、何度でも回答できます。"
        + "ポイントが下がるのは再生時間を延ばしたときだけです。",
        "Das stimmt. Rate so oft du möchtest bei der aktuellen Ausschnittlänge. Nur das Verlängern "
        + "des Ausschnitts kostet Punkte.",
        "En effet. Devinez autant de fois que vous le souhaitez à la longueur d'extrait actuelle. "
        + "Seul l'allongement de l'extrait vous coûte des points.");

    public static LocalisedString DurationSeconds { get; } = new(
        "{0} seconds",
        "{0}秒",
        "{0} Sekunden",
        "{0} secondes");

    public static LocalisedString NoPoints { get; } = new(
        "No points",
        "ポイントなし",
        "Keine Punkte",
        "Aucun point");

    public static LocalisedString Points { get; } = new(
        "{0} points",
        "{0}ポイント",
        "{0} Punkte",
        "{0} points");

    public static LocalisedString CommandHelp { get; } = new(
        "Open Bardic Memory and guess the tune.",
        "Bardic Memoryを開いて曲を当てましょう。",
        "Öffnet Bardic Memory zum Erraten der Melodie.",
        "Ouvre Bardic Memory pour deviner le morceau.");
}
