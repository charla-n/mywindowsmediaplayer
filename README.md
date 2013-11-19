Le MVVM est a priori bon.
La bibliotheque ne se fera pas via une base de donnee, mais via un fichier XML grace a Serialize pour respecter le sujet.

Todo :

	:::html

	
	Interface Bibliotheque + Code
	Interface Playlist + Code
	!DONE Gestion des images
	!DONE Binder les touches Next et Previous
	!DONE Clean le MediaPlayer sur Stop
	!DONE Corriger le warning sur le Volume
	!DONE Ajouter les fenetres Tips et About
	Rajouter les exceptions
	

Bonus :

	:::html

	!DONE Konami Code
	Binder le click sur la progressBar pour controler le media
	Faire un vrai fullscreen
	Gerer le streaming
	Enregistrer les playlists
	MVVM

Bugs :
	
	:::html
	
	Lancer un media, aller sur playlist, ajouter un media et revenir sur l'interface principale : le bouton play est mal set
	Change ce putin de FileMode dans le save de la playlist