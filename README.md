Le MVVM est a priori bon.
La bibliotheque ne se fera pas via une base de donnee, mais via un fichier XML grace a Serialize pour respecter le sujet.

Todo :

	:::html

	
	Interface Bibliotheque + Code
	!DONE Interface Playlist + Code
	!DONE Gestion des images
	!DONE Binder les touches Next et Previous
	!DONE Clean le MediaPlayer sur Stop
	!DONE Corriger le warning sur le Volume
	!DONE Ajouter les fenetres Tips et About
	!DONE OnMediaEnded appelle le média suivant dans le cas d'une playlist
	Rajouter les exceptions
	

Bonus :

	:::html

	!DONE Konami Code
	!DONE Produire un style pour le slider
	!DONE Insérer un PlaceHolder dans la classe TextBox
	!DONE Binder le click sur la progressBar pour controler le media
	Faire un vrai fullscreen
	Gerer le streaming
	!DONE Enregistrer les playlists
	MVVM

Bugs :
	
	:::html
	
	!DONE Lancer un media, aller sur playlist, ajouter un media et revenir sur l'interface principale : le bouton play est mal set
	!DONE Changer ce putin de FileMode dans le save de la playlist
	!DONE La progress bar ne fonctionne pas tres bien (temps d'une video)