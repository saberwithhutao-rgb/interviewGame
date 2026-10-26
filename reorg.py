import os,shutil
R=os.getcwd()
A=os.path.join(R,'Assets')
P=os.path.join(A,'_Project')
T=os.path.join(A,'ThirdParty')
def mv(src,dst):
    if not os.path.exists(src):
        print('skip',os.path.relpath(src,R)); return
    os.makedirs(os.path.dirname(dst),exist_ok=True)
    if os.path.exists(dst):
        print('exists',os.path.relpath(dst,R)); return
    shutil.move(src,dst); print('mv',os.path.relpath(src,R),'->',os.path.relpath(dst,R))
def mm(src,dst):
    mv(src,dst)
    if os.path.exists(src+'.meta'): mv(src+'.meta',dst+'.meta')
mm(os.path.join(P,'Script'),os.path.join(P,'Scripts'))
SC=os.path.join(P,'Scenes')
os.makedirs(SC,exist_ok=True)
for n in ['Cave Labyrinth','LevelScene','MainMenu','Map_redo01','Map_v1','MultiplayerScene','probuilder_test','Test']:
    mm(os.path.join(A,n+'.unity'),os.path.join(SC,n+'.unity'))
for n in ['ScriptableObjects','Resources']:
    mm(os.path.join(A,n),os.path.join(P,n))
mm(os.path.join(A,'Scripts','steam'),os.path.join(T,'Facepunch.Steamworks'))
ART=os.path.join(P,'Art')
os.makedirs(ART,exist_ok=True)
S=os.path.join(A,'Scenes')
if os.path.isdir(S):
    for n in sorted(os.listdir(S)):
        if n.endswith('.meta'): continue
        mm(os.path.join(S,n),os.path.join(ART,n))
for n in ['Music Loops Mini Set','Free Pack','RPG_FPS_game_assets_industrial','unity-chan!','SazenGames','Footsteps - Essentials','Zombie','Meshint Free Toon Assets','Casual Game Sounds U6']:
    mm(os.path.join(A,n),os.path.join(T,n))
print('== Assets ==')
for n in sorted(os.listdir(A)): print(n)
