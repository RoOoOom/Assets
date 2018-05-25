# -*- coding: utf-8 -*-
#FileName: dlg_welfare.py
#福利中心界面
#gWelfares

import ui
import utils
import ui_constant
import utemplateframe
import common
import net_data
import timer
import hjrpc
import time
from dlg_main_branch_target import CDlgBranchTarget
panel = None
panel1 = None
panel2 = None
from pto_comm import cREWARD_TAG as RTAG
MEDIA_MULTI_PAGE_TYPE = 5
BASE_JVBAOPEN_ITEM=(1524,1542,1530,1548,1536)
BASE_JVBAOPEN_ICON=(41005,41004,41002,41003,41001)
WELFARE_BG="mobile/common/bg_big.png"
XINFU_BG="mobile/ui_new_server/bg.png"
weeknames = ["周一","周二","周三","周四","周五","周六","周日"]
damage_level = ["一","二","三","四","五"]
limit_type={1:436,2:437,3:438,4:439,5:440}

def s_get_welfare_reward( tag, card , btn ):
	import hjrpc
	import time
	#global card_cd
	_cur_time = time.time()

	if btn.card_cd == "" or btn.card_cd == None or _cur_time - btn.card_cd > 2:
	#if card_cd == None or _cur_time - card_cd > 5:
		card = card.strip()		
		hjrpc.server.s_get_welfare_reward(tag, card)
		if btn.card_cd != "":
			btn.card_cd =  _cur_time
		#card_cd = _cur_time
	else:
		import msgbox
		msgbox.show("请勿频繁操作")

def s_refresh_welfare_menu( type ):
	#发送刷新界面的协议
	import hjrpc
	hjrpc.server.s_refresh_welfare_menu(type)
		
class CDlgRewardPnlBaseBase():
	def reg_update_func(self):
		import net_data
		net_data.hero.reg_update_hero_func(self.get_update_key(),self.update)
		
	def unreg_update_func(self):
		import net_data
		net_data.hero.unreg_update_hero_func(self.get_update_key(),self.update)
	
	def init_public(self):
		import hjrpc
		hjrpc.server.s_get_welfare_reward_info(self.get_reward_index())
		
	def get_update_key(self):
		return "WelfareRewardState%d"%self.get_reward_index()
		
	def update(self,key,value):
		pass
		#import net_data
		#state = net_data.hero.get_data(self.get_update_key())
		#if state!=None:
		#	self.btnOk.disable(not state)
		#else:
		#	self.btnOk.disable(True)
		
	def update_list(self,key,value):
		pass
		
	def get_update_list_key(self):
		return "WelfareRewardInfo%d"%self.get_reward_index()
			
	def get_update_rw_get_key(self):
		return "WelfareRewardGetInfo%d"%self.get_reward_index()
		
	def get_reward_index(self):
		return RTAG.REWARD_FIRST_TEST
				
	def on_release(self):
		self.unreg_update_func()
		
	def get_item_info(self,info):
		if type(info) == type(()) or type(info) == type([]):
			item_type,amount=info
		else:
			item_type = info
			amount = 1
			
		import item_info
		shape = item_info.item_info[item_type][item_info.SHAPE]

		_item_info ={}
		_item_info["id"]=0
		_item_info["lock"]=False
		_item_info["type"] = item_type
		_item_info["shape"] = shape
		_item_info["amount"]= amount
		return _item_info

class CDlgWelfareSign(CDlgRewardPnlBaseBase,ui.CPanel):
	def init_base(self):
		self._panel_list = {}
		self.init_public()
		self.reg_update_func()
		self.img.hide(1)
		self.frameCombin.hide(1)

	def update(self,key,value):
		import net_data
		number = int(net_data.hero.get_data(self.get_update_key()))
		type = 0

		if number and number/1000000000 >=7:
			type = 1

		template = (	
			"pnl_welfare_sign_part",
			"pnl_welfare_sign_calendar_m",
		)
		class_type = (
			CDlgWelfareSignPart1,
			CDlgWelfareSignPart2,
		)
		
		if not self._panel_list.has_key(type):
			import ui_template
			self._panel_list[type] = ui_template.create_customize(self,template[type],class_type[type])
			self._panel_list[type].set_coord(0,0)
			self._panel_list[type].zorder(-500)

		for i in range(0,2):
			if self._panel_list.has_key(i):
				if i == type:
					self._panel_list[i].hide(False)
					self._panel_list[i].show()
				else:
					self._panel_list[i].hide(True)

	def get_reward_index(self):
		return RTAG.REWARD_EVERYDAY
		
	def show(self):
		pass

class CDlgWelfareSignPart1(CDlgRewardPnlBaseBase,ui.CPanel):
	def init_base(self):
		self.init_frame_combin()
				
	def show(self):
		import net_data
		number = int(net_data.hero.get_data(self.get_update_key()))
		if number==None:
			return
		
		#第一位是签到累计数量，第二位是今天是否签到，第三位是年月
		state = number/100000000%10
		number = number/1000000000

		fn ="mobile/ui_material/qitianqiandao/%d.png"%(number+1)
		if number+1>4:
			fn = "mobile/ui_material/qitianqiandao/4.png"
		#print fn
		self.img.set_filename(fn)

		show_index = 0
		i = 0
		for frame in self.frameCombin.get_items():
			if i < number:
				frame.btnSign.hide(True)
				frame.imgSign.hide(False)			
			else:
				frame.btnSign.hide(False)
				frame.imgSign.hide(True)

			if i == number and state != 1:
				show_index = i
				frame.btnSign.disable(False)
				frame.btnSign.set_selected(False,0)
			else:
				frame.btnSign.disable(True)
				frame.btnSign.set_selected(True, 1)
			i += 1
		self.frameCombin.locate_index(show_index)
		s_refresh_welfare_menu(1)	

	def on_click_sign_btn(self,btn):
		s_get_welfare_reward(self.get_reward_index(),"",btn)
		
	def init_frame_combin(self):	
		self.frameCombin.set_add_to_manager(False)
		import ushowtemplate
		self.frameCombin.set_template("pnl_welfare_login_item",ushowtemplate.UShowTemplateFrame)
		self.frameCombin.init()
		self.frameCombin.hide_image(1)
		
		import welfare_info
		_content = welfare_info.SIGN_CONTENT
		_num = len(_content)
		self.frameCombin.add_fixed_count_items(_num)
		i = 0
		for frame in self.frameCombin.get_items():
			_gridnum = len(_content[i])
			frame.imgSignCount.set_frame(i+1)
			frame.imgSignCount.disable(1)
			x,y=frame.item1.get_coord()
			frame.msg.set_text(_content[i][0][0])
			frame.msg.disable(1)
			frame.index = i
			for index in range(0,3):
				grid = getattr(frame,"item%d"%(index+1))
				self.frameCombin.set_clickableitem_wheel_event(grid)
				if index+1 < _gridnum:
					if _content[i][index+1][1]>0:
						_item_info = self.get_item_info_by_type(_content[i][index+1][0],_content[i][index+1][1])
						grid.add_item(_item_info)
					else:
						self.set_item_icon(grid,_content[i][index+1][0],_content[i][index+1][1])
					if index>0:
						dx=0
						dy=0
						if len(_content[i][index+1])>=3:
							dx,dy = _content[i][index+1][2]
						if len(_content[i][index + 1]) >= 4:
							dx,dy = _content[i][index + 1][3]
						grid.set_coord(x + 26 + 70 * index + 1, y + 26)
						grid.set_zoom(60,dx,dy)
					grid.hide(False)
				else:
					grid.hide(True)
			
			import richformat
			import welfare_res
			i += 1
			frame.btnSign.card_cd = 0
			frame.btnSign.on_lbutton_up_arg = self.on_click_sign_btn
			
		self.frameCombin.update()
		self.frameCombin.hide(0)
		#print len(self.frameCombin.get_items()),"init_frame_combin",self.frameCombin.get_item(0).btnSign.card_cd

	def get_item_info_by_type(self,item_type,amount):
		import item_info
		shape = item_info.item_info[item_type][item_info.SHAPE]

		_item_info ={}
		_item_info["id"]=0
		_item_info["lock"]=False
		_item_info["type"] = item_type
		_item_info["shape"] = shape
		_item_info["amount"]= amount
		return _item_info

	def get_reward_index(self):
		return RTAG.REWARD_EVERYDAY

	def set_item_icon(self,item,id,type=0):
		import soldier_info,summon_info,utils
		if type==0:
			fn=utils.get_icon_fullfilename("icon/char/80x80/",id)
		else:
			fn = utils.get_icon_fullfilename("icon/char/80x80/",id)
		item.create_icon_img(fn,"guisource")
		item._icon_img.zoom(70,70)

class CDlgWelfareSignPart2(CDlgRewardPnlBaseBase,ui.CPanel):
	def init_base(self):
		self.init_calendar()
		self.bind_event()
		self._panel_list = {}
		self.change_type(0)
		self._curdateItem = None
		self.btnHelp.hide(1)

	def bind_event(self):
		self.btnHelp.on_lbutton_up = self.on_help

	def replenish_sign_confirm(self,is_ok,args):
		if is_ok:
			import hjrpc
			hjrpc.server.s_replenish_sign(int(args[0]))
	
	def on_help(self):
		import dlg_sign_help
		dlg_sign_help.show()

	def on_replenish_sign(self,btn):
		import dlg_msg
		_day = btn._day
		txtMsg = "要对%s号进行补签吗？"%_day
		dlg_msg.show(txtMsg=txtMsg,call_back_func=self.replenish_sign_confirm,args=(_day,))
	
	def update_calendar(self,curdate,list,sign_count):
		import datetime
		year = curdate/10000
		month = curdate/100%100
		day = curdate%100
		#cur_w = datetime.datetime(year,month,1).weekday() + 1
		import calendar
		monthRange = calendar.monthrange(year,month)
		maxnum = monthRange[1]
		import welfare_info
		i = 0
		j = 0
		is_start = 1
		import math
		for item in self.menu.get_item():
			if is_start == 1 and j < maxnum:
				j = j+1
				item.imgDate._day = j
				item.grid._day = j
				item.imgTag._day = j
				item.imgDate.disable(True)
				item.imgDate.hide(1)
				item.ani.hide(1)
				#刷新每个日期里的签到情况
				if sign_count > 0:
					if j in list:
						item.imgTag.hide(False)
						item.imgDate.hide(0)
						item.grid.disable(True)
						item.imgTag.disable(True)
					else:
						if j in range(math.max(1,day-5),day):
							item.imgTag.disable(False)
							item.imgDate.disable(False)
				if j == day:
					self._curdateItem=item
			else:
				item.imgDate.hide(0)
			i = i+1
	
	def init_calendar(self):
		def get_item_size():
			return 105,100

		self.menu.set_line_amount(7)
		self.menu.set_space_y(2)

		import ui_template,everyday_sign
		_year,_month,_day = self.get_month_day()
		if _month<11:
			self.imgMonth1.set_frame(_month-1)
			self.imgMonth2.hide(True)
		else:
			self.imgMonth1.set_frame(_month-11)
			self.imgMonth2.hide(False)
			self.imgMonth2.set_frame(9)

		import net_data
		list = net_data.hero.get_data(self.get_update_list_key())
		sign_count = len(list)

		self.update_sign_count(sign_count)

		_item_info = everyday_sign.everyday_sign
		for i in range(0,_day):		
			p = ui_template.create(self,"pnl_welfare_sign_calendar_article")
			p.imgTag.hide(True)
			p.get_size = get_item_size
			self.menu.add_item(p)
			# p.imgTag.on_lbutton_up_arg = self.on_replenish_sign
			# p.imgDate.on_lbutton_up_arg = self.on_replenish_sign
			p.imgDate.disable(True)
			info  = _item_info[_month][i+1]
			item_info = get_item_info(info)
			p.grid.add_item(item_info)
			p.grid.on_lclick = self.on_item_click
			p.grid._day=i+1
			p.grid.card_cd=None

	def on_item_click(self,item):
		import net_data
		number = int(net_data.hero.get_data(self.get_update_key()))
		curdate = number%100000000
		_day = curdate%100
		if item._day <_day -5 or item._day >_day :
			return
		elif item._day == _day :
			self.on_click_sign_btn(item)
		# else:
			# self.on_replenish_sign(item)
	
	def get_month_day(self):
		import net_data,calendar
		number = int(net_data.hero.get_data(self.get_update_key()))
		curdate = number%100000000
		month = curdate/100%100
		year = curdate/10000
		day = calendar.monthrange(year,month)[1]
		# print year,month,day
		return year,month,day
	
	# def get_item_info(self,info):
	#	if type(info) == type([]):
	#		item_type,amount=info[0],info[1]
	#	else:
	#		item_type = info[0]
	#		amount = 1
	#
	#	import item_info
	#	shape = item_info.item_info[item_type][item_info.SHAPE]
	#
	#
	#	_item_info ={}
	#	_item_info["id"]=0
	#	_item_info["lock"]=False
	#	_item_info["type"] = item_type
	#	_item_info["shape"] = shape
	#	_item_info["amount"]= amount
	#	return _item_info
	
	def show(self):
		import net_data
		list = net_data.hero.get_data(self.get_update_list_key())
		sign_count = len(list)
		self.update_sign_count(sign_count)
		number = int(net_data.hero.get_data(self.get_update_key()))
		state =  number/100000000%10
		curdate = number%100000000
		self.update_calendar(curdate,list,sign_count)
		self.update_btn_state(state)
		s_refresh_welfare_menu(1)	
			
	def update_reward_state(self,rw_list,sign_count):
		pass
		#rwtype = self._type
		#import welfare_info
		#need_count = welfare_info.SIGN_RW_TO_COUNT[rwtype]
		# if sign_count >= need_count:
		#	if self._type in rw_list:
		#		self.btnGetRw.hide(True)
		#		self.imgRwDone.hide(False)
		#		self.ani2.hide(True)
		#	else:
		#		self.btnGetRw.hide(False)
		#		self.btnGetRw.disable(False)
		#		self.imgRwDone.hide(True)
		#		self.ani2.hide(False)
		#		self.ani2.disable(True)
		# else:
		#	self.btnGetRw.hide(False)
		#	self.btnGetRw.disable(True)
		#	self.imgRwDone.hide(True)
		#	self.ani2.hide(True)
						
	def update_sign_count(self,sign_count):
		ten=sign_count/10
		if ten ==0:
			self.signcount2.set_frame(sign_count - ten * 10)
			self.signcount2.set_x(160)
			self.signcount1.hide(True)
		else:
			self.signcount1.set_frame(ten)
			self.signcount2.set_frame(sign_count-ten*10)
			self.signcount2.hide(False)
			self.signcount2.set_x(170)

	def update_btn_state(self,state):
		if state and state == 1:
			self._curdateItem.ani.hide(1)
		else:
			self._curdateItem.ani.hide(0)

	def get_reward_index(self):
		return RTAG.REWARD_EVERYDAY
		
	def on_click_sign_btn(self,btn):
		s_get_welfare_reward(self.get_reward_index(),"",btn)
		btn._parent.ani.hide(True)

	def on_click_getrw_btn(self,btn):
		rwtype = self._type
		s_get_welfare_reward(self.get_reward_index(),str(rwtype),btn)

	def init_reward_item(self):
		for i in range(0,6):
			_btn = getattr(self,"rwBtn%d"%(i+1))
			self.btnMenu.add_item(_btn)
			_btn._id = i
			_btn.on_lbutton_up_arg = self.on_click_type

	def change_type(self,type):
		self._type = type

	def on_click_type(self,btn):
		self.change_type(btn._id)
		import net_data
		list = net_data.hero.get_data(self.get_update_list_key())
		sign_count = len(list)
		rw_list = net_data.hero.get_data(self.get_update_rw_get_key())
		self.update_reward_state(rw_list,sign_count)

class CDlgWelfareNewRecharge2(CDlgRewardPnlBaseBase, ui.CPanel):
	def init_base(self):
		self.btn_event()
		self.init_item_add()
		self.init_public()
		self.reg_update_func()
		self.btn_event()
		self.btnGet1.card_cd = None
		self.EffectGet.disable(1)
		self.EffectGet.hide(1)

	# self.btnGet2.card_cd = None
	# self.btnCz.on_lbutton_up_arg=self.on_goto_recharge
	# self.imgMb1.set_alpha(100)
	# self.imgMb2.set_alpha(100)
	# self.imgMb1.hide(True)
	# self.imgMb2.hide(True)
	# self.imgGot1.hide(True)
	# self.imgGot2.hide(True)
	# if common.TAG_SW:
	#	self.btnTq.hide(False)
	# else:
	#	self.btnTq.hide(True)

	def get_xy(self):
		return 230,65

	# def get_item_info(self, info):
	#	item_type, amount = info[0], info[1]
	#	import item_info
	#	shape = item_info.item_info[item_type][item_info.SHAPE]
	#
	#	_item_info = {}
	#	_item_info["id"] = 0
	#	_item_info["lock"] = False
	#	_item_info["type"] = item_type
	#	_item_info["shape"] = shape
	#	_item_info["amount"] = amount
	#	return _item_info

	def on_goto_recharge(self, btn):
		import common
		common.open_recharge()

	def on_exchange_recharge(self, btn):
		import hjrpc
		hjrpc.server.s_mall_open()

	def btn_event(self):
		self.btnCz.on_lbutton_up_arg = self.on_goto_recharge
		self.btnGet1.on_lbutton_up_arg = self.on_click_get_first

	def on_click_get_first(self, btn):
		s_get_welfare_reward(self.get_reward_index(), "1", btn)

	def on_click_get_second(self, btn):
		s_get_welfare_reward(self.get_reward_index(), "2", btn)

	def init_item_add(self):
		import welfare_info
		import summon_info

		self.icon.set_head_photo(None,2183)

		for i in range(0, 3):
			info = welfare_info.NEW_FIRST_RECHARGE_CONTENT[0][i+1]
			item_info = self.get_item_info(info)
			item = getattr(self, "item%d" % (i + 1))
			item.add_item(item_info)

	def update(self, key, value):
		import net_data
		#time_list = net_data.hero.get_data(self.get_update_rw_get_key())
		#if time_list == None:
			#return
		#duration = time_list[0]
		#self.timer.set_duration(168*3600-duration)
		s_refresh_welfare_menu(1)
		number = int(net_data.hero.get_data(self.get_update_key()))
		is_can_got1 =  number
		#is_can_got2 = number%10
		if is_can_got1 == 1:
			self.btnGet1.disable(False)
			self.btnGet1.set_selected(False,0)
			self.btnCz.hide(1)
			self.EffectGet.hide(0)
		#	self.aniGet1.hide(False)
		#	self.imgMb1.hide(True)
		#	self.imgGot1.hide(True)
		else:
			self.btnCz.hide(0)
			self.EffectGet.hide(1)
			self.btnGet1.disable(True)
			self.btnGet1.set_selected(True,1)
		#	self.aniGet1.hide(True)
		#	self.imgMb1.hide(True)
		#	self.imgGot1.hide(True)

		# if is_can_got2 == 1:
		# self.btnGet2.disable(False)
		# self.aniGet2.hide(False)
		# self.imgMb2.hide(True)
		# self.imgGot2.hide(True)
		# else:
		# self.btnGet2.disable(True)
		# self.aniGet2.hide(True)
		# self.imgMb2.hide(True)
		# self.imgGot2.hide(True)

		rw_list = net_data.hero.get_data(self.get_update_list_key())
		if rw_list.count(1) > 0:
			self.btnGet1.disable(True)
			self.btnGet1.set_selected(True,1)
		#	self.btnGet1.set_filename("func/ui_material/shouchong/ylq0.zgp")
		#	self.aniGet1.hide(True)
		#	self.imgMb1.hide(False)
		#	self.imgGot1.hide(False)
		#	self.btnGet1.hide(True)

		# if rw_list.count(2) > 0:
		#	self.btnGet2.disable(True)
		#	self.btnGet2.set_filename("func/ui_material/shouchong/ylq0.zgp")
		#	self.aniGet2.hide(True)
		#	#self.imgMb2.hide(False)
		#	#self.imgGot2.hide(False)
		#	self.btnGet2.hide(True)

		# if duration >= 48*3600:
		#	if common.TAG_SW:
		#		self.lblDesc.set_text("提取580元宝可额外获得价值1980元宝的10元首充礼包")
		#	else:
		#		self.lblDesc.set_text("充值58元可额外获得价值1980元宝的10元首充礼包")
		# else:
		#
		#	if common.TAG_SW:
		#		self.lblDesc.set_text("48小时内提取580元宝可额外获得价值2980元宝的10元首充礼包")
		#	else:
		#		self.lblDesc.set_text("48小时内充值58元可额外获得价值2980元宝的10元首充礼包")


	def show(self):
		pass

	def get_reward_index(self):
		return RTAG.REWARD_NEW_FIRST_RECHARGE2

	def on_release(self):
		self.unreg_update_func()
		#self.timer.pause()

class CDlgWelfareCard(ui.CPanel):
	def init_base(self):
		self.bind_event()

	def bind_event(self):
		self.pnl1.btnGet.on_lbutton_up_arg = self.on_click_get_jinka_reward
		self.pnl2.btnGet.on_lbutton_up_arg = self.on_click_get_zhuanka_reward
		#self.pnl1.btnGet.on_lbutton_up_arg = self.on_click_get_reward
		self.pnl1.btnBuy.on_lbutton_up_arg = self.on_click_buy_card
		self.pnl1.btnUpgrade.on_lbutton_up_arg = self.on_click_upgrade_card

	def get_xy(self):
		return 180,-10

	def on_click_get_btn(self,btn):
		text = self.edit.get_text()
		if text!="":
			s_get_welfare_reward(RTAG.REWARD_MEDIA,text,btn)

	def request_update_info(self):
		import hjrpc
		hjrpc.server.s_get_welfare_card_menu_info()

	def init_pnl(self,index,b,time):
		_pnl = getattr(self, "pnl%d" % (index))
		if index==1:
			for i in range(1,10):
				img=getattr(_pnl,"CImage%d"%i)
				img.hide(b)
			_pnl.btnUpgrade.hide(not b)
			_pnl.btnBuy.hide(b)
			_pnl.btnGet.hide(not b)
			_pnl.desc1.hide(b)
			_pnl.desc2.hide(b)
			_pnl.desc3.hide(not b)
			_pnl.desc1.set_text("每日可领取#10810元宝,持续30天")
			_pnl.desc2.set_text("购买后立得:#108100元宝")
			_pnl.desc3.set_text("今日可领#10810元宝,截止时间:%s"%time)
		else:
			_pnl.desc3.hide(0)
			_pnl.desc3.set_text("今日可领16#108,截止时间:%s" % time)
		_pnl.jinyuekatishi.hide(not b)


	# def on_btn_buy(self):
	#	import common
	#	if common.SHENCHA_TAG:
	#		import msgbox
	#		msgbox.show("充值系统暂时未开")
	#		return
	#	if common.YH_TAG:
	#		utils.pay_yh(2,"yueka",9.8)
	#	else:
	#		import webbrowser
	#		webbrowser.open(utils.get_recharge_html())

	def update_one_info(self,type,time,rw,rwid):
		index = 1
		self.pnl1.hide(1)
		self.pnl2.hide(1)
		if not (rwid == 50802 or rwid == 0):
			index = 2
		_pnl = getattr(self, "pnl%d" % (index))
		_pnl.hide(False)
		if time != "":
			self.init_pnl(index,True,time)
			if rw == 1:
				_pnl.btnGet.set_selected(True,1)
				_pnl.btnGet.disable(True)
			else:
				_pnl.btnGet.set_selected(False,0)
				_pnl.btnGet.disable(False)
			s_refresh_welfare_menu(1)
		else:
			self.init_pnl(index,False,time)

		#
		#	import net_data
		#	import hjrpc
		#	host = net_data.get_hostid()
		#	if host >= net_data.get_new_hostid():
		#	#if host >= 1260:
		#		hjrpc.server.s_get_welfare_reward_info(RTAG.REWARD_NEW_FIRST_RECHARGE)
		#		hjrpc.server.s_get_welfare_reward_info(RTAG.REWARD_NEW_FIRST_RECHARGE2)

	def update_info(self,info_list):
		for i in range(0,1):
			print info_list[i]["RewardId"]
			self.update_one_info((i+1),info_list[i]["EndTime"],info_list[i]["RewardStatus"],info_list[i]["RewardId"])
		s_refresh_welfare_menu(1)

	def check_count_and_do_buy(self,count,type):
		pass
		#import welfare_info
		#conf = welfare_info.CARD_NAME[(type-1)]
		#if count < conf[1]:
		#	def check_pass_func(is_ok,args):
		#		if is_ok:
		#			#import webbrowser
		#			#import welfare_res
		#			#webbrowser.open(welfare_res.charge_web_addr)
		#			import common,webbrowser
		#			webbrowser.open(utils.get_recharge_html())

		#	import dlg_msg
		#	import welfare_res
		#	error_msg = welfare_res.charge_msg%conf[1]
		#	dlg_msg.show(txtMsg=error_msg,call_back_func=check_pass_func,txtOK = "前往充值",args=())
		#else:
		#	import hjrpc
		#	hjrpc.server.s_buy_welfare_card(type)

	def on_click_upgrade_card(self,btn):
		import dlg_msg,hjrpc
		def call_back_func(is_ok,args):
			if is_ok:
				hjrpc.server.s_buy_welfare_card(1)
		dlg_msg.show(txtMsg = "月卡升级",call_back_func = call_back_func,args={},type=10,txtOK = "升级",txtCancel = "取消")
		# import hjrpc
		# hjrpc.server.s_buy_welfare_card(1)
		# import dlg_msg
		#
		# def callback(is_ok, args=None):
		#	if is_ok:
		#		import hjrpc
		#		hjrpc.server.s_buy_welfare_card(1)
		# dlg_msg.show(txtMsg="消耗#R268#n元宝可升级月卡!延长#R60#n天收益时间。", call_back_func=callback)

	def on_click_buy_card(self,btn):
		import hjrpc
		hjrpc.server.s_buy_welfare_card(1)

	def on_click_get_jinka_reward(self,btn):
		import hjrpc
		hjrpc.server.s_get_welfare_card_reward(1)

	def on_click_get_zhuanka_reward(self,btn):
		import hjrpc
		hjrpc.server.s_get_welfare_card_reward(1)

	def show(self):
		self.request_update_info()

	def get_reward_index(self):
		pass

class CDlgWelfareUpgrade(CDlgRewardPnlBaseBase,ui.CPanel):
	def init_base(self):
		self.btn_count = 0
		self.init_public()
		self.init_lv_frame()
		self.reg_update_func()

	def update(self,key,value):
		import net_data
		curpro = net_data.hero.get_data(self.get_update_key())
		rw_list = net_data.hero.get_data(self.get_update_rw_get_key())
		self.update_lv_frame(curpro,rw_list)
		count = self.btn_count
		self.update_reward_btn(count)
		s_refresh_welfare_menu(1)
	
	def update_lv_frame(self,curpro,rw_list):
		import welfare_info
		_lv = welfare_info.UPGARDE_LV
		_content = welfare_info.UPGRADE_RW_CONTENT

		show_list=[]
		for index in range(len(_lv)):
			if not index in rw_list:
				show_list.append(index)
		self.btn_count=show_list
		# item_list = [_content[i:i+6] for i in range(0, len(_content), 6)]
		self.lstItem.clear()
		self.lstItem.add_fixed_count_items(len(show_list))
		i=0
		for index in show_list:
			item_display = self.lstItem.get_item(i)
			item_display.get_size = self.get_lst_item_size
			item_display.imgLvHundred.disable(True)
			item_display.imgLvTen.disable(True)
			item_display.imgLvUnit.disable(True)
			item_display.imgRwDone.hide(1)
			hundred = _lv[index] / 100
			ten = _lv[index] / 10
			unit = 0
			if hundred > 0:
				item_display.imgLvHundred.hide(False)
				item_display.imgLvHundred.set_frame(hundred)
				ten = (_lv[index] - hundred * 100) / 10
			else:
				item_display.imgLvHundred.hide(True)

			if _lv[index] % 10 != 0:
				unit = 5
			item_display.imgLvHundred.set_frame(hundred)
			item_display.imgLvTen.set_frame(ten)
			item_display.imgLvUnit.set_frame(unit)
			item_display.btnGetRw.card_cd = 0
			item_display.btnGetRw._id = index
			item_display.btnGetRw.on_lbutton_up_arg = self.on_click_get_rw_btn

			_gridnum = len(_content[index])
			for n in range(0, 4):
				grid = getattr(item_display, "rwItem%d" % (n + 1))
				self.lstItem.set_clickableitem_wheel_event(grid)
				if n < _gridnum:
					_item_info = self.get_item_info(_content[index][n])
					grid.add_item(_item_info)
					grid._bg.disable(1)
					grid.hide(False)
			i += 1
		self.lstItem.update()

	def get_lst_item_size(self):
		return 750, 100
		
	def init_lv_frame(self):
		import ushowtemplate
		self.lstItem.set_template("pnl_welfare_upgrade_article", ushowtemplate.UShowTemplateFrame)
		self.lstItem.hide_image(True)
		self.lstItem.init()

	def on_click_get_rw_btn(self,btn):
		s_get_welfare_reward(self.get_reward_index(),str(btn._id),btn)

	def update_reward_btn(self,btn_count):
		import net_data
		pro = net_data.hero.get_data(self.get_update_key())
		rw_list = net_data.hero.get_data(self.get_update_rw_get_key())

		if  len(rw_list)==16:
			global panel
			del panel._config[3]
			panel.update_menu()
			panel.on_click_type(panel.lstMenu.get_item(0))

		i=0
		for index in btn_count:
			item_display=self.lstItem.get_item(i)
			item_display.imgRwDone.disable(True)
			if pro and rw_list != None:
				if index < pro:
					item_display.btnGetRw.hide(False)
					item_display.btnGetRw.disable(False)
					item_display.btnGetRw.set_selected(False, 0)
					item_display.imgRwDone.hide(True)
				else:
					item_display.btnGetRw.hide(False)
					item_display.btnGetRw.disable(True)
					item_display.btnGetRw.set_selected(True,1)
					item_display.imgRwDone.hide(True)
			else:
				item_display.btnGetRw.hide(False)
				item_display.btnGetRw.disable(True)
				item_display.btnGetRw.set_selected(True, 1)
				item_display.imgRwDone.hide(True)
			i+=1

	def get_reward_index(self):
		return RTAG.REWARD_UPGRADE
	
	def show(self):
		pass

class CDlgWelfareConsume(CDlgRewardPnlBaseBase, ui.CPanel):
	def init_base(self):
		self.init_welfare_consume_item()
		import net_data
		net_data.hero.unreg_update_hero_func(self.get_update_rw_get_key(), self.update)
		self.init_public()
		self.bind_event()
		self.reg_update_func()

		if common.TAG_SW:
			self.imgTip.set_filename("func/ui_material/xiaofei/jryxfds1.zgp")

	def get_lst_item_size(self):
		w, h = self.lstItem.get_size()
		return 750, 100

	def init_welfare_consume_item(self):
		import ushowtemplate
		self.lstItem.set_template("pnl_wefare_consume_item_m", ushowtemplate.UShowTemplateFrame)
		self.lstItem.set_add_to_manager(False)
		# self.lstItem.set_selecter(False)
		self.lstItem.hide_image(True)
		self.lstItem.init()

		zgp = [
			"mobile/ui_material/xiaofeisongli/1.zgp",
			"mobile/ui_material/xiaofeisongli/2.zgp",
			"mobile/ui_material/xiaofeisongli/3.zgp",
			"mobile/ui_material/xiaofeisongli/4.zgp",
			"mobile/ui_material/xiaofeisongli/5.zgp",
			"mobile/ui_material/xiaofeisongli/dcsl.zgp",
		]

		self.lstItem.add_fixed_count_items(len(zgp))

		import welfare_info
		for i in range(len(zgp)):
			item_display = self.lstItem.get_item(i)
			item_display.get_size = self.get_lst_item_size
			item_display.set_rect(750, 100)
			item_display.imgDay.set_filename(zgp[i])
			grid = 1
			for info in welfare_info.CONSUME_ITEMS[i]:
				item_info = self.get_item_info(info)
				item = getattr(item_display, "itemConsume%d" % grid)
				self.lstItem.set_clickableitem_wheel_event(item)
				item.gird = grid
				item.add_item(item_info)
				grid += 1
			if i == 5:
				item_display.imgDay.set_coord(10, 33)
		self.lstItem.update()

	def bind_event(self):
		import common, welfare_info
		if common.SHENCHA_TAG:
			self.btnCharge.hide(True)
		self.btnCharge.on_lbutton_up = self.on_charge_up

	def update(self, key, value):
		import net_data, utils
		rw_list = net_data.hero.get_data(self.get_update_rw_get_key())
		if rw_list == None:
			return
		ConsumeTotal, ConsumeCurrent, ConsumeToday, duration = rw_list[0], rw_list[1], rw_list[
			2], utils.safe_to_int(rw_list[3])

		self.imgTodayConsume.set_text(str(ConsumeToday))
		if ConsumeTotal >= 5:
			ConsumeTotal = 6

		for i in range(0, 6):
			_panel = self.lstItem.get_item(i)
			_panel.btnSuo.disable(1)
			_panel.imgRwDone.disable(1)
			if ConsumeCurrent < i + 1:
				if i + 1 > ConsumeTotal:
					_panel.btnSuo.hide(False)
					_panel.lblNo.hide(False)
					_panel.btnGet.hide(True)
					_panel.imgRwDone.hide(True)
				else:
					_panel.btnSuo.hide(True)
					_panel.lblNo.hide(True)
					_panel.btnGet.hide(False)
					_panel.imgRwDone.hide(True)
			else:
				_panel.btnSuo.hide(True)
				_panel.lblNo.hide(True)
				_panel.btnGet.hide(True)
				_panel.imgRwDone.hide(False)
		if duration:
			self.lblLeftTime.set_duration(duration)
			import time
			endtime = time.time() + duration
			self.lblEndTime.set_text(time.strftime("%m月%d日%H时%M分", time.localtime(endtime)))

	def show(self):
		import hjrpc
		hjrpc.server.s_get_welfare_reward_info(self.get_reward_index())

	def on_charge_up(self):
		import common
		common.open_recharge()

	def get_reward_index(self):
		return RTAG.REWARD_CONSUME

	def on_click_consume_btn(self, btn):
		s_get_welfare_reward(self.get_reward_index(), "", btn)
		s_refresh_welfare_menu(1)

	def on_release(self):
		self.unreg_update_func()
		self.lblLeftTime.pause()

class CDlgWelfareGiftPack(CDlgRewardPnlBaseBase, ui.CPanel):
	def init_base(self):
		import net_data
		net_data.hero.unreg_update_hero_func(self.get_update_rw_get_key(), self.update)
		self.init_public()
		self.bind_event()
		self.reg_update_func()
		self._info_dic={}
		self._info_dic[1] = {}
		self._info_dic[2] = {}
		#self._info_dic[i]["select_count"] = {}
		#self._info_dic[i]["limite_count"] = {}
		#self._info_dic[i]["price"] ={}
		#self._info_dic[i]["type"] = {}
		self._moneys={}
		#self._info_dic[i]["money_type"]={}
		for i,name in enumerate(["JinQian","YuanBao"]):
			self._info_dic[i+1]["money"] = net_data.hero.get_data(name)
			net_data.hero.reg_update_hero_func(name, self.update_current)
			self.update_current(name, self._info_dic[i+1]["money"])
			self._info_dic[i+1]["select_count"] = 0
			self._info_dic[i+1]["limite_count"] = 0
			self._info_dic[i+1]["price"] = 0
			self._info_dic[i+1]["type"] = 0
			self._info_dic[i+1]["money_type"] = 0
		global giftpack_inited
		giftpack_inited = True
		#ani = get_ani_bytag(RTAG.REWARD_GIFTPACK)
		#if ani:
			#ani.hide(True)
		s_refresh_welfare_menu(2)

	def update_current(self, key, value):
		value=utils.safe_to_int(value)
		if key=="JinQian":
			self.lblCurrent1.set_price_text(value)
			self._info_dic[1]["money"]=value
		else:
			self.lblCurrent2.set_price_text(value)
			self._info_dic[2]["money"]=value

	def bind_event(self):
		import common, welfare_info
		if common.SHENCHA_TAG:
			self.btnCharge.hide(True)
		for i in range(1,3):
			lblCount=getattr(self,"lblCount%d"%i)
			btnBuy = getattr(self, "btnBuy%d" % i)
			btnDel = getattr(self, "btnDel%d" % i)
			btnAdd = getattr(self, "btnAdd%d" % i)
			lblCount.bind_event()
			lblCount.set_keyboard_dx(-50)
			btnBuy._index = i
			btnDel._index = i
			btnAdd._index = i
			btnBuy.on_lbutton_up_arg=self.on_buy_up
			btnDel.on_lbutton_up_arg = self.change_count
			btnAdd.on_lbutton_up_arg = self.change_count
		self.lblCount1.after_msg_char = lambda:self.after_msg_char(1)
		self.lblCount2.after_msg_char = lambda:self.after_msg_char(2)

	def after_msg_char(self,index):
		import net_data,time,math
		# rw_list = net_data.hero.get_data(self.get_update_rw_get_key())
		# if rw_list == None:
		#	return
		# [StartTime, EndTime, type, amount, total] = rw_list
		#
		# import math
		# count = math.max(0, math.min(amount, utils.safe_to_int(getattr(self,"lblCount%d"%index).get_text())))
		#
		# desc, name, price, _ = self.get_shop_bytype(type)
		#
		# total_price = count * price
		# if amount == total:
		#	total_price = math.max(0, (count - 1) * price + math.ceil(price * 0.3))
		#	#self.imgZhekou.hide(False)
		# else:
		#	pass
		#	#self.imgZhekou.hide(True)
		lblTotal=getattr(self,"lblTotal%d"%index)
		lblCount = getattr(self, "lblCount%d" % index)
		text = int(lblCount.get_text())
		self._info_dic[index]["select_count"] = math.min(text, self._info_dic[index]["limite_count"])
		if self._info_dic[index]["limite_count"] == self._info_dic[index]["total"] and index == 2:
			total_price= math.max(0, (self._info_dic[index]["select_count"] - 1) * self._info_dic[index]["price"] + math.ceil(self._info_dic[index]["price"] * 0.3))
		else:
			total_price=self._info_dic[index]["select_count"] * self._info_dic[index]["price"]
		lblTotal.set_price_text(total_price)
		lblCount.set_price_text(self._info_dic[index]["select_count"])

	def change_count(self,btn):
		import math
		index=btn._index
		if btn.get_text()=="-":
			self._info_dic[index]["select_count"]-=1
			if self._info_dic[index]["select_count"]<0:
				self._info_dic[index]["select_count"]=0
		else:
			self._info_dic[index]["select_count"] += 1
			if self._info_dic[index]["select_count"]>self._info_dic[index]["limite_count"]:
				self._info_dic[index]["select_count"]=self._info_dic[index]["limite_count"]
		getattr(self,"lblCount%d"%index).set_text(self._info_dic[index]["select_count"])
		if self._info_dic[index]["limite_count"] == self._info_dic[index]["total"] and index==2:
			total_price= math.max(0, (self._info_dic[index]["select_count"] - 1) * self._info_dic[index]["price"] + math.ceil(self._info_dic[index]["price"] * 0.3))
		else:
			total_price=self._info_dic[index]["select_count"] * self._info_dic[index]["price"]
		getattr(self, "lblTotal%d"%index).set_price_text(total_price,self._info_dic[index]["money"]>=total_price)

	def get_shop_bytype(self, type):
		import npc_shop_name_info
		import npc_shop_info
		sellId = None

		desc, name, price, sid = None, None, None, None
		for id, item_dict in npc_shop_info.npc_shop_info.items():
			if item_dict.has_key(type):
				price = item_dict[type][npc_shop_info.PRICE]
				sellId = id
				break
		for id, shop_dict in npc_shop_name_info.info.items():
			sellList = shop_dict[npc_shop_name_info.SHOP_LIST]
			if sellId in sellList:
				desc = shop_dict[npc_shop_name_info.DESC]
				desc=desc.strip()
				desc = desc.strip('"')
				name = shop_dict[npc_shop_name_info.NAME]
				sid = id
		return desc, name, price, sid

	def update(self, key, value):
		import net_data,time,npc_shop_info,item_func,math
		rw_list = net_data.hero.get_data(self.get_update_rw_get_key())
		if rw_list == None:
			return
		[StartTime, EndTime, type, amount, total] = rw_list
		money_type=npc_shop_info.npc_shop_info["shuqi2"][type][3][0]

		strEndTime = time.localtime(EndTime)
		self.intEndMonth.set_number(strEndTime[1])
		self.intEndDay.set_number(strEndTime[2])
		self.intEndHour.set_number(strEndTime[3])
		self.intEndMinute.set_number(strEndTime[4])

		desc, name, price, _ = self.get_shop_bytype(type)
		if money_type==1:
			self.itemGift1.add_item(item_func.get_ui_item_info(type, 1))
			self.lblAmount1.set_text("%d/%d" % (total - amount, total))
			self._info_dic[1]["limite_count"]=amount
			self._info_dic[1]["price"]=price
			self._info_dic[1]["type"] = type
			self._info_dic[1]["total"] = total
			self._info_dic[1]["money_type"] = money_type
		elif money_type==72:
			self.itemGift2.add_item(item_func.get_ui_item_info(type, 1))
			self.lblAmount2.set_text("%d/%d" % (total - amount, total))
			self._info_dic[2]["limite_count"]=amount
			self._info_dic[2]["price"] = price
			self._info_dic[2]["type"]= type
			self._info_dic[2]["total"] = total
			self._info_dic[2]["money_type"]=money_type
			if amount == total:
				self.imgZhekou.hide(False)
			else:
				self.imgZhekou.hide(True)
				self.lblTotal2.set_price_text(price*self._info_dic[2]["select_count"],self._info_dic[2]["money"]>=price)

		self.lblDesc.set_text(desc)
		#self.imgTitle.set_filename(name)
		s_refresh_welfare_menu(1)

	def show(self):
		import hjrpc
		hjrpc.server.s_get_welfare_reward_info(self.get_reward_index())

	def on_buy_up(self,item):
		index=item._index
		import net_data,msgbox,math
		rw_list = net_data.hero.get_data(self.get_update_rw_get_key())
		if rw_list == None:
			return
		# [StartTime, EndTime, type, amount, total] = rw_list
		if self._info_dic[index]["select_count"] > self._info_dic[index]["limite_count"]:
			msgbox.show("已达到今日最大购买数量。")
			return
		#desc, name, price, sid = self.get_shop_bytype(type)
		import hjrpc
		if not self._info_dic[index]["select_count"]:
			msgbox.show("请选择数量。")
			return
		total_price = math.max(0,utils.safe_to_int(getattr(self,"lblTotal%d"%index).get_text()))
		#print self._info_dic[index]["type"], self._info_dic[index]["select_count"], total_price,self._info_dic[index]["money_type"]
		hjrpc.server.s_npc_seller_buy(0, "giftpack1", self._info_dic[index]["type"], self._info_dic[index]["select_count"], total_price,self._info_dic[index]["money_type"])
		self.show()

	def get_reward_index(self):
		return RTAG.REWARD_GIFTPACK

	def on_release(self):
		self.unreg_update_func()
		import net_data
		for i, name in enumerate(["JinQian", "YuanBao"]):
			net_data.hero.unreg_update_hero_func(name, self.update_current)

class CDlgWelfareLucky(CDlgRewardPnlBaseBase, ui.CPanel):
	def init_base(self):
		self.init_public()
		self.reg_update_func()
		self._timer_id = None
		self._count=0
		self.init_grid()
		self.btnExtract.on_lbutton_up_arg = self.on_extract_up
		# self.boxCk.on_lbutton_up_arg = self.on_extract_up
		# self.boxCk.on_mouse_enter = self.on_extract_mouse_enter
		# self.boxCk.on_mouse_leave = self.on_extract_mouse_leave
		# self.boxCk.set_alpha(1)

	# for i in range(0,4):
	#	_ani = getattr(self,"ani%d"%(i+1))
	#	_ani.hide(1)

	# def get_item_info(self, info):
	#	if type(info) == type(()):
	#		item_type, amount = info
	#	else:
	#		item_type = info
	#		amount = 1
	#
	#	import item_info
	#	shape = item_info.item_info[item_type][item_info.SHAPE]
	#
	#	_item_info = {}
	#	_item_info["id"] = 0
	#	_item_info["lock"] = False
	#	_item_info["type"] = item_type
	#	_item_info["shape"] = shape
	#	_item_info["amount"] = amount
	#	return _item_info

	def init_grid(self):
		items = []
		import welfare_info
		li = welfare_info.NewLotteryRewardExtractItems_20161022
		for item in li:
			if type(item[0]) == type([]):
				items.append((item[0][0], item[1]))
			else:
				items.append((item[0], item[1]))

		for i in range(0, len(items)):
			if hasattr(self, "grid%d" % (i + 1)):
				grid = getattr(self, "grid%d" % (i + 1))
				_item_info = self.get_item_info(items[i])
				grid.add_item(_item_info)
				grid._bg.hide(1)
				grid._cover.hide(1)

	def on_extract_up(self, btn):
		self.btnExtract.disable(1)
		#self.boxCk.disable(1)
		#self.ani_btn.hide(1)

		import hjrpc
		hjrpc.server.s_event_lottery_roll()

	# for i in range(0,4):
	#	_ani = getattr(self,"ani%d"%(i+1))
	#	_ani.hide(1)

	def update_ani(self):
		for i in range(0, 12):
			grid = getattr(self, "grid%d" % (i + 1))
			if grid._ani_effect:
				grid._ani_effect.hide(i != self._show_index)

		import welfare_info
		li = welfare_info.NewLotteryRewardExtractItems_20161022
		item = li[self._show_index]
		item_type = None
		if type(item[0]) == type([]):
			if self._real_item_index:
				item_type = item[0][self._real_item_index]
			else:
				item_type = item[0][0]
		else:
			item_type = item[0]

		import item_info
		shape = item_info.item_info[item_type][item_info.SHAPE]
		self.icon.set_path("guisource")
		self.icon.set_filename("icon/item/100x100/%d.zgp" % shape)
		#print self.icon.get_coord()
		self.icon.set_coord(200,276)

	def on_timer(self):
		if not self.is_valid():
			if self._timer_id != None:
				self._timer_id = None
			return 0

		self._all_elips_time += 1
		self._elips_time += 1

		# 做个速度曲线公式吧
		speed = 0.0002 * self._all_elips_time * self._all_elips_time - 0.05 * self._all_elips_time + 2
		self._speed = speed

		self._speed = min(self._speed, 30)
		self._speed = max(self._speed, 0.01)

		if self._elips_time > self._speed:
			self._elips_time = 0
			self._show_index += 1
			if self._show_index > 11:
				self._show_index = 0

		self.update_ani()

		if self._all_elips_time > 40 and self._show_index == self._open_result_index:
			import hjrpc
			hjrpc.server.s_event_lottery_reward()
			self._timer_id = None
			self.btnExtract.disable(0)
			#self.boxCk.disable(0)
			#self.ani_btn.hide(0)
			s_refresh_welfare_menu(2)
			return 0

	def on_get_item_result(self, index,count):
		#self.boxCk.disable(1)
		self._open_result_index = 0
		self._real_item_index = 0
		import welfare_info
		items = []
		self._open_result_index = index - 3

		if self._timer_id == None:
			self._all_elips_time = 0
			self._elips_time = 0
			self._speed = 2
			self._show_index = 0
			import timer
			for i in range(0, 12):
				grid = getattr(self, "grid%d" % (i + 1))
				grid.play_item_use_effect("mobile/item/select.zgp", coord=(-3, -3), loop=True, isUse=False)
				grid._ani_effect.set_frame(1)
				grid._ani_effect.hide(1)
				grid._ani_effect.zorder(0)
			self._timer_id = timer.start_timer(2, self.on_timer)
			self._count=count
			self.timer.set_text(count)
			global panel
			item=panel.get_welfare_by_tag(self.get_reward_index())
			if item:
				item.aniMenu.hide(not self._count)

	def show(self):
		pass

	def get_reward_index(self):
		return RTAG.REWARD_LUCKY_PRIZE

	def update(self,key,value):
		import time,event_info
		rw_list = net_data.hero.get_data(self.get_update_rw_get_key())
		[StartTime, EndTime, Timer, _, _] = rw_list
		strEndTime = time.localtime(EndTime)
		strStartTime = time.localtime(StartTime)
		self.start_time.set_text("开始:"+str(strStartTime[1])+"月"+str(strStartTime[2])+"日"+str(strStartTime[2])+"时"+str(strStartTime[3])+"分")
		self.end_time.set_text("结束:"+str(strEndTime[1]) + "月" + str(strEndTime[2]) + "日" + str(strEndTime[2]) + "时" + str(strEndTime[3]) + "分")
		self.desc.set_text(event_info.event_info[353][event_info.CONTEXT])
		#self.btnExtract.disable(not Timer)
		self.timer.set_text(Timer)
		self._count=Timer

class CDlgLimitTime(CDlgRewardPnlBaseBase, ui.CPanel):
	def init_base(self):
		self.item_list.set_template("pnl_xiangou")
		self.item_list.set_direction(1)
		self.item_type = 17001
		self.init_public()
		self.reg_update_func()
		self.bind_event()
		#self.update_item()

	def show(self):
		pass

	def bind_event(self):
		pass

	def get_reward_index(self):
		return RTAG.REWARD_LIMIT

	def unreg_update_func(self):
		import net_data
		net_data.hero.unreg_update_hero_func("WelfareRewardState%d"%self.get_reward_index(),self.update)
		#net_data.hero.unreg_update_hero_func(self.get_update_rw_get_key(), self.update)

	def reg_update_func(self):
		import net_data
		net_data.hero.reg_update_hero_func("WelfareRewardState%d"%self.get_reward_index(),self.update)
		#net_data.hero.reg_update_hero_func(self.get_update_rw_get_key(), self.update)

	def update(self,key,value):
		import net_data
		self.good_list=net_data.hero.get_data(self.get_update_list_key())
		self.price_list=net_data.hero.get_data(self.get_update_rw_get_key())
		self.everyday_pack=net_data.hero.get_data(self.get_update_key())
		self.update_item()

	def update_item(self):
		import dlg_shop
		self.good_list.sort()
		import welfare_info,item_info,net_data
		grade =net_data.hero.get_data("Grade")
		self.item_list.clear()
		self.item_list.add_fixed_count_items(len(self.good_list))
		for i,type in enumerate(self.good_list):
			item_display = self.item_list.get_item(i)
			item_display.get_size = lambda :(250,369)
			item_display.set_rect(250, 369)
			item_display.bg.disable(1)
			item_display.lblName.disable(1)
			info=welfare_info.LIMIT_TIME_CONTENT[type]
			for j in range(1,5):
				item=getattr(item_display,"item%d"%j)
				self.item_list.set_clickableitem_wheel_event(item)
				if j>len(info):
					item.hide(1)
				else:
					item.hide(0)
					if i==0 and j+1==2:
						self.item_type=info[j+1][0]
						good_info = dlg_shop.data.get_good(1, self.item_type)
						b = 1 if good_info and good_info["number"]==0 else 0
						item_display.btnOk.set_selected(b,b)
					item.add_item(get_item_info(info[j+1]))
			item_display.lblName.set_text(info[0])
			item_display.btnOk._type = type
			item_display.btnOk.card_cd = None
			item_display.btnOk.on_lbutton_up_arg=self.btn_ok
			item_display.btnOk.set_text(info[1])
			if i==1:
				item_display.btnOk.set_selected(self.everyday_pack,self.everyday_pack)
			if type>6:
				item_display.btnOk.set_selected(1 if grade < self.good_list[i] else 0,1 if grade < self.good_list[i] else 0)
		self.item_list.update()

	def btn_ok(self,btn):
		import msgbox
		if btn.get_text()=="前往商城" and btn.is_selected():
			msgbox.show("本周已没有购买次数,请下周再来")
			return
		import dlg_shop
		if 1<=btn._type<=3 and self.item_type:
			dlg_shop.show(3,self.item_type)
			return
		s_get_welfare_reward(self.get_reward_index(), str(btn._type), btn)

	def get_item_info_by_type(self,item_type,amount=1):
		import item_info
		shape = item_info.item_info[item_type][item_info.SHAPE]

		_item_info ={}
		_item_info["id"]=0
		_item_info["lock"]=False
		_item_info["type"] = item_type
		_item_info["shape"] = shape
		_item_info["amount"]= amount
		return _item_info

class CDlgServerTop(CDlgRewardPnlBaseBase,ui.CPanel):
	def init_base(self):
		import welfare_info
		self.init_public()
		self.reg_update_func()
		self._time_list={}
		self._panel_list={}
		self.current_panel=None
		self.menu.set_direction(2)
		self.menu.set_line_amount(1)
		self.menu.set_space_y(5)

		i=0
		for k,v in welfare_info.LANGYABANG_RW_CONTENT.iteritems():
			button=ui.CRichTextButton(self, 100, 100, "mobile/common/btn_109.zgp", ui_constant.SKIN_PATH,text=k,color=0x436288,font=common.FONT_SONGTI_20)
			button.zorder(-300)
			button._index=i
			button.on_lbutton_up_arg = self.change_tag
			self.menu.add_item(button)
			i+=1
		self.change_tag(self.menu.get_item()[0])

	def change_tag(self,btn):
		self.menu.set_selected(btn)
		import welfare_info,event_info
		for _panel in self._panel_list.itervalues():
			_panel.hide(1)
		if self._panel_list.has_key(btn.get_text()):
			self._panel_list[btn.get_text()].hide(0)
		else:
			self._panel_list[btn.get_text()]=utemplateframe.UTemplateFrame(self,39,270,760,300)
			self._panel_list[btn.get_text()].set_template("pnl_new_server_top_item")
			self._panel_list[btn.get_text()].zorder(-300)
			_info = welfare_info.LANGYABANG_RW_CONTENT[btn.get_text()]
			self._panel_list[btn.get_text()].event_id = _info[0]
			self._panel_list[btn.get_text()].add_fixed_count_items(len(_info)-1)
			import richformat,event_info,hjrpc
			for i in range(0,len(_info)-1):
				item_display = self._panel_list[btn.get_text()].get_item(i)
				item_display.get_size = lambda :[750,100]
				item_display.set_rect(750,100)
				item_display.CFrame.disable(1)
				item_display.rank.set_text(_info[i+1][0])
				for n in range(0, 4):
					grid = getattr(item_display, "item%d" % (n + 1))
					self._panel_list[btn.get_text()].set_clickableitem_wheel_event(grid)
					if n+1 < len(_info[i+1]):
						_item_info = self.get_item_info(_info[i+1][n+1])
						grid.add_item(_item_info)
						grid._bg.disable(1)
						grid.hide(False)
					else:
						grid.hide(1)
			self._panel_list[btn.get_text()].update()
		event_id = self._panel_list[btn.get_text()].event_id
		self.desc.set_text(event_info.event_info[event_id][event_info.CONTEXT])
		if not self._time_list.has_key(event_id):
			hjrpc.server.s_get_welfare_reward(self.get_reward_index(), str(event_id))
		else:
			self.update_time(self._time_list[event_id])

	def update(self,key,value):
		time_list=net_data.hero.get_data(self.get_update_rw_get_key())
		event_id=net_data.hero.get_data(self.get_update_key())
		if not  self._time_list.has_key(event_id):
			self._time_list[event_id]=time_list
		self.update_time(self._time_list[event_id])

	def update_time(self,list):
		[StartTime, EndTime, _, _, _] = list
		strEndTime = time.localtime(EndTime)
		strStartTime = time.localtime(StartTime)
		self.time.set_text("2018年"+str(strStartTime[1])+"月"+str(strStartTime[2])+"日"+str(strStartTime[3]) + "时"+str(strStartTime[4]) + "分-"+str(strEndTime[1]) + "月" + str(strEndTime[2]) + "日"+str(strEndTime[3]) + "时"+str(strEndTime[4]) + "分")

	def show(self):
		pass

	def get_xy(self):
		return 245,70

	def get_reward_index(self):
		return RTAG.REWARD_LANGYABANG

class CDlgLimitBoss(CDlgRewardPnlBaseBase,ui.CPanel):
	def init_base(self):
		self.init_public()
		self.reg_update_func()

	def update(self,key,value):
		import math
		time_list = net_data.hero.get_data(self.get_update_rw_get_key())
		type_list = net_data.hero.get_data(self.get_update_list_key())
		count = math.max(10 - (net_data.target.get_counter_info("time_limit_boss_reward") or 0),0)
		if len(time_list)==0:
			time_list=[0,0,0,0,0]
		if len(type_list)==0:
			time_list=[1,2,3,4,5]
		self.update_item(type_list,time_list,count)

	def update_item(self,type_list,time_list,count):
		import event_info
		self.lblRemain.set_text("今日剩余奖励次数：%d次"%count)
		self.lblRemain.set_color(0x354975 if count else 0xff0000)
		for i in range(1,6):
			time=time_list[i - 1]
			event_type=limit_type[type_list[i-1]]
			rewar_items=event_info.event_info[event_type][event_info.RWITEM]
			tips_str = event_info.event_info[event_type][event_info.CONTEXT]
			item=getattr(self,"item%d"%i)
			item.imgCanChallenge.hide(not time_list[i-1])
			item.lblTime.set_duration(time if time else 0)
			item.lblTime.hide(not time_list[i - 1])
			item.lblDesc.set_text("还未出现" if not time_list[i-1] else "后将离开")
			item.lblDesc.set_coord(97 if not time_list[i-1] else 166,52)
			item.btnTips._tips = tips_str
			item.btnTips.on_lbutton_up_arg = self.on_tips
			item.lblName.set_text(event_info.event_info[event_type][event_info.NAME])
			fn = utils.get_icon_fullfilename("icon/item/100x100/",event_info.event_info[event_type][event_info.ICON])
			item.event_icon.set_filename(fn)
			item.btnGo._type=i
			item.btnGo.on_lbutton_up_arg = self.go_to_scene
			for j,reward in enumerate(rewar_items):
				if hasattr(item, "item%d" % (j+1)):
					grid = getattr(item, "item%d" % (j+1))
					grid.add_item(get_item_info(rewar_items[j]))

	def on_tips(self,btn):
		import dlg_tips
		dlg_tips.show("common", ("挑战说明", btn._tips))

	def go_to_scene(self,btn):
		import hjrpc
		hjrpc.server.s_enter_maxcount_scene(btn._type)
		self._parent.hide(1)

	def get_xy(self):
		return 275,65

	def get_reward_index(self):
		return RTAG.REWARD_LIMITBOSS

	def show(self):
		pass

class CDlgServerBoss(CDlgRewardPnlBaseBase,ui.CPanel):
	def init_base(self):
		self.selected_index=utils.get_cur_wek() - 1
		self.server_day=utils.get_cur_wek() - 1
		self._init = False
		self._tips_str = ""
		self._timer=None
		self._round=0
		self.boss_infos = None
		self.bind_event()
		self.init_list_title()
		self.init_public()
		self.reg_update_func()
		import dlg_tips
		title = "关于伤害排行榜"
		desc = "1、排行榜只记录伤害前100名的玩家。#r2、排行榜记分采用队伍记分制，同一队伍下所有人的#r分数都与队伍得分相同。#r3、每个排行榜的信息将会单独保留一周时间。#r4、每天只有前10次挑战的伤害会被记录。"
		self.tip.on_lbutton_up =lambda:dlg_tips.show("common",(title,desc))
		self.tipReward.on_lbutton_up = lambda: dlg_tips.show("common", ("档次说明", self._tips_str))

	def bind_event(self):
		self.btnL.on_lbutton_up=lambda :self.change_index(-1)
		self.btnR.on_lbutton_up = lambda: self.change_index(1)
		self.btnJoin.on_lbutton_up=lambda :hjrpc.server.s_lookfor_worldboss()


	def init_list_title(self):
		import event_info
		self.desc.set_text(event_info.event_info[432][event_info.CONTEXT])
		column_info = ( "排名", "名称", "伤害",)
		column_type = (1, 1, 1)
		title_info_list = []
		for index, text in enumerate(column_info):
			title_info_list.append({"TEXT": text, "FONT": common.FONT_SONGTI_20, "COLOR":0x52638d})
		self.list.set_title(title_info_list)
		self.list.set_col_type(column_type)
		self.list.draw_bg()
		self.list.set_row_dw(0)
		self.list.set_title_text_dy(3)

	def change_index(self,type):
		self.selected_index+=type
		if self.selected_index<0:
			self.selected_index=0
		elif self.selected_index>len(self.boss_infos)-1:
			self.selected_index=len(self.boss_infos)-1
		self.update_boss()

	def start_timer(self):
		self.stop_timer()
		import timer
		self._timer = timer.start_timer_sec(1, self.on_timer)

	def on_timer(self):
		if not self._timer:return
		self._round+=1
		if self._round%15==0:
			self.init_public()

	def stop_timer(self):
		if self._timer != None:
			timer.stop_timer(self._timer)
			self._timer = None
			self._round = 0

	def after_hide(self,b):
		self.stop_timer()

	def after_release(self):
		self.stop_timer()

	def show(self):
		self.start_timer()

	def get_xy(self):
		return 250,60

	def get_reward_index(self):
		return RTAG.REWARD_WORLDBOSS

	def update(self,key,value):
		pass
		#[StartTime, EndTime, _, _, _] = net_data.hero.get_data(self.get_update_rw_get_key())
		#self.server_day=net_data.hero.get_data(self.get_update_key())

		#strEndTime = time.localtime(EndTime)
		#strStartTime = time.localtime(StartTime)
		#self.time.set_text("2018年"+str(strStartTime[1])+"月"+str(strStartTime[2])+"日-"+str(strEndTime[1]) + "月" + str(strEndTime[2]) + "日")

		# if not self._init:
		# 	self.selected_index = self.server_day
		# 	self.update_boss()
		# 	self._init=True

	def update_info(self,infos):
		self.boss_infos = infos
		self.update_boss()

	def update_boss(self):
		self.btnL.hide(self.selected_index==0)
		self.btnR.hide(self.selected_index ==len(self.boss_infos)-1)
		if len(self.boss_infos)<=self.selected_index:
			return
		self.name.set_text(self.boss_infos[self.selected_index]["name"])
		self.shape.set_shape(self.boss_infos[self.selected_index]["type"])
		if self.server_day==self.selected_index:
			self.btnJoin.disable(0)
			self.btnJoin.set_selected(False,0)
		else:
			self.btnJoin.disable(1)
			self.btnJoin.set_selected(True,1)
		self.openTime.set_text("%s 12:00开启"%weeknames[self.selected_index])
		if self.boss_infos[self.selected_index]["level"]==0:
			self.myLevel.set_text("无")
		else:
			self.myLevel.set_text("第%s档" % damage_level[self.boss_infos[self.selected_index]["level"]-1])
		self._tips_str = self.boss_infos[self.selected_index]["level_tips"]
		self.CProgressHpAuto.set_cur(self.boss_infos[self.selected_index]["curhp"])
		self.CProgressHpAuto.set_max(self.boss_infos[self.selected_index]["maxhp"])
		self.CProgressHpAuto.set_min(0)
		if self.boss_infos[self.selected_index]["curhp"]==-1 or self.boss_infos[self.selected_index]["maxhp"]==-1:
			self.CProgressHpAuto.set_str("?/?")
		for i , info in enumerate(self.boss_infos[self.selected_index]["rewards"]):
			item=getattr(self,"item%d"%(i+1))
			item.add_item(get_item_info(info))

		self.list.clear()
		my_damage = 0
		for index, infos in enumerate(self.boss_infos[self.selected_index]["ladder"]):
			i=index+1
			uid=infos.get("UID", "")
			name = infos.get("Name", "")
			damage = infos.get("Damage", 0)
			if uid == net_data.hero.get_data("Id"):
				my_damage = damage
			temp=[i,name,damage]
			info_list = []
			for index,text in enumerate(temp):
				dict = {}
				dict["COLOR"] =  0x3e366c
				dict["FONT"] =  common.FONT_SONGTI_18
				dict["TEXT"] = utils.safe_to_str(text)
				info_list.append(dict)
			self.list.add_item(info_list, uid, redraw=False)
		self.list.redraw()
		self.myDamage.set_text(my_damage)

class CDlgServerBox(CDlgRewardPnlBaseBase,ui.CPanel):
	def init_base(self):
		self.init_public()
		self.reg_update_func()
		import event_info
		desc=event_info.event_info[431][event_info.CONTEXT]
		rewards = event_info.event_info[431][event_info.RWITEM]
		self.desc.set_text(desc)
		for i,info in enumerate(rewards):
			item = getattr(self,"item%d"%(i+1))
			item.add_item(get_item_info(rewards[i]))

	def get_xy(self):
		return 260,55

	def show(self):
		pass

	def update(self,key,value):
		[StartTime, EndTime, _, _, _] = net_data.hero.get_data(self.get_update_rw_get_key())
		strEndTime = time.localtime(EndTime)
		strStartTime = time.localtime(StartTime)
		self.time.set_text("2018年"+str(strStartTime[1])+"月"+str(strStartTime[2])+"日-"+str(strEndTime[1]) + "月" + str(strEndTime[2]) + "日")
		#self.end_time.set_text("结束:"+str(strEndTime[1]) + "月" + str(strEndTime[2]) + "日" + str(strEndTime[2]) + "时" + str(strEndTime[3]) + "分")

	def get_reward_index(self):
		return RTAG.REWARD_WORLDBOX

class CDlgServerBonus(CDlgRewardPnlBaseBase,ui.CPanel):
	def init_base(self):
		self.init_public()
		self.reg_update_func()
		import dlg_shop,dlg_tips
		self.btnOk.on_lbutton_up=lambda:dlg_shop.show(4)
		self.tips.on_lbutton_up =lambda:dlg_tips.show("common",("关于返还元宝",self.get_tips()))

	def get_tips(self):
		return "1、充值将以账号为统计单位，同一账号下的所有角色充值#r均计入该账号的累计充值中。\
		#r2、公测开启后，该账号登入新服创建的第一个角色，#r将会获得所有元宝返还。\
		#r3、1.5倍返还元宝不包括充值赠送及其他活动赠送的元宝。\
		#r4、本次返还元宝不设上限，所有充值均会获得1.5倍返还。"

	def get_xy(self):
		return 260,55

	def show(self):
		pass

	def update(self,key,value):
		import math
		state = net_data.hero.get_data(self.get_update_key())
		self.lblRecharge.set_text(state)
		self.lblReward.set_text(int(math.floor(state*1.5)))
		# strEndTime = time.localtime(EndTime)
		# strStartTime = time.localtime(StartTime)
		# self.time.set_text("2018年"+str(strStartTime[1])+"月"+str(strStartTime[2])+"日-"+str(strEndTime[1]) + "月" + str(strEndTime[2]) + "日")
		#self.end_time.set_text("结束:"+str(strEndTime[1]) + "月" + str(strEndTime[2]) + "日" + str(strEndTime[2]) + "时" + str(strEndTime[3]) + "分")

	def get_reward_index(self):
		return RTAG.REWARD_BONUS

class CDlgKaifuGiftPack(CDlgRewardPnlBaseBase,ui.CPanel):
	def init_base(self):
		self.init_public()
		self.reg_update_func()

	def get_xy(self):
		return 260,55

	def show(self):
		pass

	def get_reward_index(self):
		return RTAG.REWARD_KAIFUGIFTPACK

	def update_item(self):
		for i in range(0,4):
			item = getattr(self, "item%d" % (i + 1))
			btn = getattr(self, "btn%d" % (i + 1))
			item.add_item(get_item_info(self.good_list[i]))
			#print self.good_list[i],self.good_list[i]
			btn.set_selected(self.price_list[i],self.price_list[i])
			btn._index=i+1
			btn.card_cd = None
			btn.on_lbutton_up_arg=self.on_btn_ok

	def on_btn_ok(self,btn):
		s_get_welfare_reward(self.get_reward_index(), str(btn._index), btn)

	def update(self,key,value):
		import time
		self.good_list = net_data.hero.get_data(self.get_update_list_key())
		self.price_list = net_data.hero.get_data(self.get_update_rw_get_key())
		strStartTime = time.localtime(net_data.hero.get_data(self.get_update_key())/10**10)
		strEndTime = time.localtime(net_data.hero.get_data(self.get_update_key()) % 10 ** 10)
		self.time.set_text("2018年" + str(strStartTime[1]) + "月" + str(strStartTime[2]) + "日-" + str(strEndTime[1]) + "月" + str(strEndTime[2]) + "日")
		self.update_item()



class CDlgGshxCheckIn(CDlgRewardPnlBaseBase,ui.CPanel):
	def init_base(self):
		self.init_public()
		self.reg_update_func()
		#import hjrpc
		#hjrpc.server.s_get_welfare_reward_info(self.get_reward_index())
		#import hjrpc
		#hjrpc.server.s_get_welfare_reward_info(self.get_reward_index())
	
	def show(self):
		import hjrpc
		hjrpc.server.s_get_welfare_reward_info(self.get_reward_index())

	def update(self, key, value):
		import net_data
		rw_list = pro = net_data.hero.get_data(self.get_update_list_key())
		if rw_list==None:
			return
		spend,total_times,today_times = rw_list
		self.init_frame_combin(spend)

		if spend > 0:
			fn = "mobile/ui_material/qitianqiandao/11.png"
		else:
			fn = "mobile/ui_material/qitianqiandao/12.png"

		self.img.set_filename(fn)

		global panel
		item=panel.get_welfare_by_tag(self.get_reward_index())
		if item and spend < 0:
			item.btnMenu.set_text("萌新福利")

		i = 0
		show_index = 0
		for frame in self.frameCombin.get_items():
			if i < total_times:
				frame.btnSign.hide(True)
				frame.imgSign.hide(False)			
			else:
				frame.btnSign.hide(False)
				frame.imgSign.hide(True)

			if i == total_times and today_times != 1:
				show_index = i
				frame.btnSign.disable(False)
				frame.btnSign.set_selected(False,0)
			else:
				frame.btnSign.disable(True)
				frame.btnSign.set_selected(True, 1)
			i += 1
		self.frameCombin.locate_index(show_index)
		s_refresh_welfare_menu(1)	

	def on_click_sign_btn(self,btn):
		s_get_welfare_reward(self.get_reward_index(),"",btn)
		
	def init_frame_combin(self, spend):	
		self.frameCombin.set_add_to_manager(False)
		import ushowtemplate
		self.frameCombin.set_template("pnl_welfare_login2_item",ushowtemplate.UShowTemplateFrame)
		self.frameCombin.init()
		self.frameCombin.hide_image(1)
		
		import welfare_info
		if spend > 0:
			_content = welfare_info.GSHX_CHECKIN_CONTENT
		else:
			_content = welfare_info.GSHX_CHECKIN_CONTENT2
		_num = len(_content)
		self.frameCombin.add_fixed_count_items(_num)
		i = 0
		for frame in self.frameCombin.get_items():
			_gridnum = len(_content[i])
			frame.imgSignCount.set_frame(i+1)
			frame.imgSignCount.disable(1)
			x,y=frame.item1.get_coord()
			for index in range(0,5):
				grid = getattr(frame,"item%d"%(index+1))
				self.frameCombin.set_clickableitem_wheel_event(grid)
				if index+1 < _gridnum:
					if _content[i][index+1][1]>0:
						_item_info = self.get_item_info_by_type(_content[i][index+1][0],_content[i][index+1][1])
						grid.add_item(_item_info)
					else:
						self.set_item_icon(grid,_content[i][index+1][0],_content[i][index+1][1])
					#if index>0:
						#dx=0
						#dy=0
						#if len(_content[i][index+1])>=3:
						#	dx = _content[i][index+1][2]
						#if len(_content[i][index + 1]) >= 4:
						#	dx = _content[i][index + 1][3]
						#grid.set_coord(x + 26 + 70 * index + 1, y + 26)
						#grid.set_zoom(60,dx,dy)
					grid.hide(False)
				else:
					grid.hide(True)
			
			import richformat
			import welfare_res
			i += 1
			frame.btnSign.card_cd = 0
			frame.btnSign.on_lbutton_up_arg = self.on_click_sign_btn
			
		self.frameCombin.update()
		self.frameCombin.hide(0)
		#print len(self.frameCombin.get_items()),"init_frame_combin",self.frameCombin.get_item(0).btnSign.card_cd

	def get_item_info_by_type(self,item_type,amount):
		import item_info
		shape = item_info.item_info[item_type][item_info.SHAPE]

		_item_info ={}
		_item_info["id"]=0
		_item_info["lock"]=False
		_item_info["type"] = item_type
		_item_info["shape"] = shape
		_item_info["amount"]= amount
		return _item_info

	def set_item_icon(self,item,id,type=0):
		import soldier_info,summon_info,utils
		if type==0:
			fn=utils.get_icon_fullfilename("icon/char/80x80/",id)
		else:
			fn = utils.get_icon_fullfilename("icon/char/80x80/",id)
		item.create_icon_img(fn,"guisource")
		item._icon_img.zoom(70,70)

	def get_reward_index(self):
		return RTAG.REWARD_GSHX_CHECKIN


class CDlgFriendRecruit(CDlgRewardPnlBaseBase,ui.CPanel):
	def init_base(self):
		self.inited = False
		self.be_recruited = False
		self.proccess_list = None
		self.btn_active_list = None
		self.tag = 0
		self.init_public()
		self.reg_update_func()
		self.bind_event()
		import net_data
		self.frCopyCode.lblId.set_text(net_data.hero.get_data("Id"))
		self.frCopyCode.lblNum.set_text("%d人"%0)
		self.btnMyRecruit.tag = 0		
		self.btnInputCode.tag = 1
		self.recruit_id = 0
		self.menu.add_item(self.btnMyRecruit,False)
		self.menu.add_item(self.btnInputCode,False)
		self.frdRecruitItem1.prgRecruit.set_range(1,0,0)		
		self.on_selected_tag(self.btnMyRecruit)		
		self.inited = True
		net_data.hero.reg_update_hero_func("RecruitorName",self.show_comfirm_dialog)
		pass

	def bind_event(self):		
		self.btnTips.on_lbutton_up = self.show_rule_tips
		self.btnMyRecruit.on_lbutton_up_arg = self.on_selected_tag
		self.btnInputCode.on_lbutton_up_arg = self.on_selected_tag
		self.frCopyCode.btnOk.on_lbutton_up = self.on_copy_code
		self.frdInputCode.btnOk.on_lbutton_up = self.on_comfirm_code
		for i in range(1,3):
			frdRecruitItem = getattr(self,"frdRecruitItem%d"%i)
			if frdRecruitItem:
				frdRecruitItem.btnGet.on_lbutton_up_arg = self.on_get_reward
				frdRecruitItem.btnGet.card_cd = None
				frdRecruitItem.btnGet.disable(True)
				frdRecruitItem.btnGet.set_selected(True,1)
				_x,_y = frdRecruitItem.btnGet.get_coord()
				_w,_h = frdRecruitItem.btnGet.get_size()
				frdRecruitItem.aniOk.zoom(_w + 20,_h)
				frdRecruitItem.aniOk.set_coord(_x-20,_y-10)					
				

	def show(self):		
		pass

	def update(self,key,value):		
		import net_data
		self.proccess_list =net_data.hero.get_data(self.get_update_list_key())			
		self.btn_active_list = net_data.hero.get_data(self.get_update_rw_get_key())

		print self.proccess_list
		print "===================="
		print self.btn_active_list
		print "========================"
		
		self.update_reward_info()
		pass

	def update_reward_info(self):
		import net_data
		self.proccess_list =net_data.hero.get_data(self.get_update_list_key())			
		self.btn_active_list = net_data.hero.get_data(self.get_update_rw_get_key())
		if not self.proccess_list or not self.btn_active_list:
			print "two empty data list"
			return 

		if net_data.hero.get_data("Grade") > 5 and not self.proccess_list[2]:
			self.btnInputCode.hide(True)

		for i in range(1,3):
			frdRecruitItem = getattr(self,"frdRecruitItem%d"%i)
			if frdRecruitItem:
				_index = i 
				if self.tag == 0:
					_index = i-1
				elif self.tag == 1:
					_index = i+1
				_cur_value = self.proccess_list[_index] > frdRecruitItem.prgRecruit.get_max() and frdRecruitItem.prgRecruit.get_max() or self.proccess_list[_index]
				frdRecruitItem.prgRecruit.set_cur(_cur_value)
				frdRecruitItem.aniOk.hide(True)
				#0表示没达到领取条件，1表示可领取，2表示已领取
				if self.btn_active_list[_index] == 1:
					frdRecruitItem.btnGet.set_text("点击领取")
					frdRecruitItem.btnGet.disable(False)
					frdRecruitItem.btnGet.set_selected(False,0)
					frdRecruitItem.aniOk.hide(False)
				elif self.btn_active_list[_index] == 2:
					frdRecruitItem.btnGet.set_text("已领取")
					frdRecruitItem.btnGet.disable(False)
					frdRecruitItem.btnGet.set_selected(True,1)
				else:
					frdRecruitItem.btnGet.set_text("点击领取")
					frdRecruitItem.btnGet.disable(True)
					frdRecruitItem.btnGet.set_selected(True,1)

		if self.tag == 0:
			import net_data
			num = net_data.hero.get_data(self.get_update_key())
			recruited_num = "%d人"%num
			self.frCopyCode.lblNum.set_text(recruited_num)			
			playerId = net_data.hero.get_data("Id")
			self.frCopyCode.lblId.set_text(playerId)
		elif self.tag == 1:
			if self.btn_active_list[2]:
				self.be_recruited = True
				self.frdInputCode.hide(True)
			pass

		#self.frdRecruitItem1.prgRecruit.set_cur()
		#self.frdRecruitItem2.prgRecruit.set_cur()
		pass

	def get_item_info_by_type(self,item_type,amount):
		import item_info
		shape = item_info.item_info[item_type][item_info.SHAPE]

		_item_info ={}
		_item_info["id"]=0
		_item_info["lock"]=False
		_item_info["type"] = item_type
		_item_info["shape"] = shape
		_item_info["amount"]= amount
		return _item_info

	#如果奖励是召唤兽，只需要显示头像即可
	def set_item_icon(self,item,shape,):
		import utils
		fn=utils.get_icon_fullfilename("icon/char/80x80/",shape)

		item.create_icon_img(fn,"guisource")
		item._icon_img.zoom(70,70)

	#0是我的招募，1是填写邀请码
	def switch_tag_reward_info(self):
		import welfare_info
		recruit_item_info = []

		if self.tag==0:
			self.imgBg.set_filename("mobile/ui_recruit/bg1.png")
			self.frdRecruitItem1.lblTitle.set_text("招募奖励")
			self.frdRecruitItem2.lblTitle.set_text("进阶奖励")
			self.frdRecruitItem1.lblProccess.set_text("招募进度")
			self.frdRecruitItem2.lblProccess.set_text("招募进度")
			self.frdRecruitItem2.prgRecruit.set_range(5,0,0)
			recruit_item_info.append(welfare_info.RECRUIT_CONTENT[0])
			recruit_item_info.append(welfare_info.RECRUIT_CONTENT[1])			
			self.frdRecruitItem1.btnGet.num = "1"
			self.frdRecruitItem2.btnGet.num = "2"
		elif self.tag==1:
			self.imgBg.set_filename("mobile/ui_recruit/bg2.png")
			self.frdRecruitItem1.lblTitle.set_text("被招募奖励")
			self.frdRecruitItem2.lblTitle.set_text("进阶奖励")
			self.frdRecruitItem1.lblProccess.set_text("达成进度")
			self.frdRecruitItem2.lblProccess.set_text("达成进度")
			self.frdRecruitItem2.prgRecruit.set_range(15,0,0)
			recruit_item_info.append(welfare_info.RECRUIT_CONTENT[2])
			recruit_item_info.append(welfare_info.RECRUIT_CONTENT[3])
			self.frdRecruitItem1.btnGet.num = "3"
			self.frdRecruitItem2.btnGet.num = "4"

		for i in range(1,3):
			_i = i -1 
			for m in range(1,3):
				frdRecruitItem = getattr(self,"frdRecruitItem%d"%i)
				if frdRecruitItem:
					frdRecruitItem.lblDesc.set_text(recruit_item_info[_i][0])
					_item = getattr(frdRecruitItem,"item%d"%m)
					_box = getattr(frdRecruitItem,"box%d"%m)
					if _item:						
						_item.del_item()
						if recruit_item_info[_i][m][1]>0:
							_item_info = self.get_item_info_by_type(recruit_item_info[_i][m][0],recruit_item_info[_i][m][1])					
							_item.add_item(_item_info)
							_box._info = _item_info
							_box.on_lbutton_up_arg = self.on_check_reward_item
							_box.disable(False)
						else:
							self.set_item_icon(_item,recruit_item_info[_i][m][0])
							_box.disable(True)					
		if self.inited:
			self.update_reward_info()
		pass
	def get_reward_index(self):
		return RTAG.REWARD_RECRUIT

	#0是我的招募，1是填写邀请码
	def on_selected_tag(self,btn):
		self.menu.set_selected(btn)		
		self.frCopyCode.hide(btn.tag!=0)
		if not self.be_recruited:
			self.frdInputCode.hide(btn.tag!=1)
		self.tag = btn.tag
		self.switch_tag_reward_info()

	def on_check_reward_item(self,btn):
		import dlg_tips
		dlg_tips.show("item",(0,btn._info),None,False)

	def on_get_reward(self,btn):
		print "on_get_reward"
		s_get_welfare_reward(self.get_reward_index(),btn.num,btn)
		pass

	def on_comfirm_code(self):
		print "on_comfirm_code"
		playerId = self.frdInputCode.edit.get_text()
		if playerId.isdigit():
			self.recruit_id = int(playerId.strip())
			hjrpc.server.s_get_recruitor_name(self.recruit_id)
			#self.show_comfirm_dialog()
		else:
			import dlg_msg
			dlg_msg.show("您输入的邀请码有误，请重新输入",None,None,dlg_msg.dlg_type_list.NORMAL_TYPE,"重新输入")
		pass

	def show_comfirm_dialog(self,key,value):
		import net_data,dlg_msg
		recruit_name = net_data.hero.get_data("RecruitorName")
		dlg_msg.show("您即将接受#G%s#n的邀请，每位玩家只能填写一次邀请码，是否确认？"%recruit_name,self.accept_recruit,self.recruit_id)

	def accept_recruit(self,sure,playerId):
		if sure:
			hjrpc.server.s_accept_recruit_id(playerId)

	def on_copy_code(self):
		print "on_copy_code"
		pass

	def unreg_update_func(self):
		super(self.__class__,self).unreg_update_func()
		import net_data
		net_data.hero.unreg_update_hero_func("RecruitorName",self.show_comfirm_dialog)

	def show_rule_tips(self):
		print "show_rule_tips"
		import dlg_tips
		dlg_tips.show("common",("招募规则","#r1.玩家等级≤5级可以被招募#r2.玩家等级≥0级可以招募好友"))
		pass

# class CDlgWelfareFirstCharge(CDlgRewardPnlBaseBase,ui.CPanel):
#	def init_base(self):
#		self.cur_libao_list = [0,1,2]
#		self.cur_selected_id = None
#		self.init_public()
#		self.reg_update_func()
#		self.init_rw_menu()
#		self.bind_event()
#		self.btnGet.card_cd = None
#
#
#	def update(self,key,value):
#		import net_data
#		list = pro = net_data.hero.get_data(self.get_update_list_key())
#		rw_list = net_data.hero.get_data(self.get_update_rw_get_key())
#		curpro = list[0]
#
#		for i in range(0, curpro):
#			if i not in rw_list:
#				curpro = i
#				break
#		import math
#		item = self.menu._items[0]
#		self.on_change_btn(None, math.min(curpro,4) - item._id - 1)
#
#		for item in self.menu._items:
#			if item._id == curpro or curpro == 7:
#				self.on_click_lb(item.boxBg)
#
#		self.update_progress(list)
#		self.update_reward_btn()
#		s_refresh_welfare_menu(1)
#
#		#if list[0] > len(rw_list):
#			#panel.ani1.hide(False)
#			#panel.ani1.disable(True)
#		#kelse:
#			#panel.ani1.hide(True)
#			#panel.ani1.disable(False)
#
#	def get_item_info(self,info):
#		if type(info) == type(()):
#			item_type,amount=info
#		else:
#			item_type = info
#			amount = 1
#
#		import item_info
#		shape = item_info.item_info[item_type][item_info.SHAPE]
#
#		_item_info ={}
#		_item_info["id"]=0
#		_item_info["lock"]=False
#		_item_info["type"] = item_type
#		_item_info["shape"] = shape
#		_item_info["amount"]= amount
#		return _item_info
#
#
#
#	def update_progress(self,list):
#		import welfare_info
#		curpro = list[0]
#		yb = list[1]
#		ybnum = welfare_info.CHARGE_LV_TO_YB
#		size = len(ybnum)
#		pro = curpro
#		if curpro >= size:
#			pro = pro - 1
#
#		max = ybnum[pro]
#
#		self.barProgress.set_max(max)
#		self.barProgress.disable(True)
#		self.barProgress.set_cur(yb)
#
#		left = (max-yb) > 0 and (max-yb) or 0
#
#		import richformat
#		import welfare_res
#		if pro == 0:
#			richformat.set_richlabel_text_ex(self.msg,welfare_res.recharge_msg_first%(left))
#		else:
#			richformat.set_richlabel_text_ex(self.msg,welfare_res.recharge_msg%(left,(pro+1)))
#		self.msg.set_color(0xAF6C23)
#		dirlist = welfare_info.CHARGE_LB_TITLE_LIST_DIR
#		tiplist = welfare_info.CHARGE_LB_LIST_TIPS
#
#		self.imgCurLv.set_filename(dirlist[pro])
#		self.imgCurLv.set_tips(tiplist[pro])
#
#	def update_reward_btn(self):
#		import net_data
#		list = pro = net_data.hero.get_data(self.get_update_list_key())
#		rw_list = net_data.hero.get_data(self.get_update_rw_get_key()) or []
#
#		curpro = 0
#		if list != None:
#			curpro = list[0]
#
#		if self.cur_selected_id == None:
#			return
#
#		self.btnGet.disable(False)
#
#		if self.cur_selected_id < curpro:
#			if self.cur_selected_id in rw_list:
#				self.btnGet.set_filename("func/ui_material/b7/btn_yilingqu.zgp")
#				self.btnGet.disable(True)
#				self.btnGet.set_select(True,1)
#				self.ani.hide(True)
#			else:
#				self.btnGet.set_filename("func/ui_material/b4/btn_lingqujiangli.zgp")
#				self.btnGet.disable(False)
#				self.btnGet.set_select(False, 0)
#				self.ani.hide(False)
#		else:
#			self.btnGet.set_filename("func/ui_material/b4/btn_lingqujiangli.zgp")
#			self.ani.hide(True)
#			self.btnGet.disable(True)
#
#	def bind_event(self):
#		self.btnUp.on_lbutton_up_arg = self.on_click_up_btn
#		self.btnDown.on_lbutton_up_arg = self.on_click_down_btn
#		self.btnDs.on_lbutton_up_arg = self.on_click_ds_btn
#		self.btnCz.on_lbutton_up_arg = self.on_click_cz_btn
#		self.btnGet.on_lbutton_up_arg = self.on_click_get_btn
#
#	def init_rw_menu(self):
#		def get_item_size():
#			return 203,155
#
#		self.menu.set_line_amount(3)
#
#		import ui_template
#		import welfare_info
#		dirlist = welfare_info.CHARGE_LB_LIST_DIR
#		tiplist = welfare_info.CHARGE_LB_LIST_TIPS
#
#		for i in range(0,3):
#			p = ui_template.create(self,"pnl_welfare_charge_article")
#			p._id = i
#			p.boxBg.on_lbutton_up_arg = self.on_click_lb
#			p.boxBg.set_tips(tiplist[i])
#			p.imgLv.set_filename(dirlist[i])
#			p.get_size = get_item_size
#			self.menu.add_item(p)
#
#		self.on_click_lb(self.menu._items[0].boxBg)
#
#	def on_click_lb(self, box):
#		item = box.get_parent()
#		id = utils.safe_to_int(item._id)
#		if self.cur_selected_id == id:
#			return
#		self.cur_selected_id = id
#
#		for i in range(0,3):
#			if item == self.menu._items[i]:
#				self.menu._items[i].boxBg.set_color(0xD9D919)
#				self.menu._items[i].boxBg.set_alpha(40)
#			else:
#				self.menu._items[i].boxBg.set_alpha(0)
#
#		import welfare_info
#		self.imgName.set_filename(welfare_info.CHARGE_LB_TITLE_LIST_DIR[id])
#
#		info = welfare_info.CHARGE_LB_ITMES[id]
#		for i in range(0, 5):
#			item = getattr(self, "item%d"%(i+1))
#			if len(info) > i:
#				item_info = self.get_item_info(info[i])
#				item.add_item(item_info)
#				item.hide(False)
#			else:
#				item.hide(True)
#		self.update_reward_btn()
#
#
#	def on_click_get_btn(self,btn):
#		#id = btn._id
#		#import hjrpc
#		#hjrpc.server.s_get_welfare_reward(self.get_reward_index(),str(id))
#		if self.cur_selected_id == None:
#			return
#		s_get_welfare_reward(self.get_reward_index(),str(self.cur_selected_id),btn)
#
#	def on_change_btn(self,btn,arg):
#		import welfare_info
#		dirlist = welfare_info.CHARGE_LB_LIST_DIR
#		max = len(dirlist)
#
#		if arg == 1 and self.cur_libao_list[2] == (max-1):
#			return
#
#		if arg == -1 and self.cur_libao_list[0] == 0:
#			return
#
#		import welfare_info
#		dirlist = welfare_info.CHARGE_LB_LIST_DIR
#		tiplist = welfare_info.CHARGE_LB_LIST_TIPS
#
#		i = 0
#		for item in self.menu._items:
#			#id = item.btnGet._id + arg
#			#item.btnGet._id = id
#			id = item._id + arg
#			item._id = id
#			item.imgLv.set_filename(dirlist[id])
#			#item.imgLv.set_tips(tiplist[id])
#			item.boxBg.set_tips(tiplist[id])
#			item.boxBg.set_alpha(0)
#			#item.imgDone.hide(True)
#			self.cur_libao_list[i] = id
#			i = i + 1
#
#		self.update_reward_btn()
#
#	def on_click_up_btn(self,btn):
#		self.on_change_btn(btn,1)
#
#	def on_click_down_btn(self,btn):
#		self.on_change_btn(btn,-1)
#
#	def on_click_ds_btn(self,btn):
#		import hjrpc
#		hjrpc.server.s_mall_open()
#
#	def on_click_cz_btn(self,btn):
#		#import webbrowser
#		#import welfare_res
#		#webbrowser.open(welfare_res.charge_web_addr)
#		import common,webbrowser
#		webbrowser.open(utils.get_recharge_html())
#
#	def get_reward_index(self):
#		return RTAG.REWARD_FIRST_RECHARGE
#
#	def show(self):
#		global panel
#		if panel and (panel._cur_reward_index == None or panel._cur_reward_index !=  RTAG.REWARD_FIRST_RECHARGE):
#			self.init_public()
#
#	#def show(self):
#	#	pass

# class CDlgWelfareNewRecharge(CDlgRewardPnlBaseBase,ui.CPanel):
#	def init_base(self):
#		self.init_item_add()
#		self.init_public()
#		self.reg_update_func()
#		self.btn_event()
#		self.btnGet1.card_cd = None
#		#self.btnGet2.card_cd = None
#		#self.imgMb1.set_alpha(100)
#		#self.imgMb2.set_alpha(100)
#		#self.imgMb1.hide(True)
#		#self.imgMb2.hide(True)
#		#self.imgGot1.hide(True)
#		#self.imgGot2.hide(True)
#
#	def get_item_info(self,info):
#		item_type,amount = info[0],info[1]
#		import item_info
#		shape = item_info.item_info[item_type][item_info.SHAPE]
#
#		_item_info ={}
#		_item_info["id"]=0
#		_item_info["lock"]=False
#		_item_info["type"] = item_type
#		_item_info["shape"] = shape
#		_item_info["amount"]= amount
#		return _item_info
#
#	def on_goto_recharge(self,btn):
#		import webbrowser
#		import common
#
#		if common.SHENCHA_TAG:
#			import msgbox
#			msgbox.show("充值系统暂时未开")
#			return
#
#		webbrowser.open(utils.get_recharge_html())
#
#	def on_exchange_recharge(self,btn):
#		import hjrpc
#		hjrpc.server.s_mall_open()
#
#	def btn_event(self):
#		self.btnTq.on_lbutton_up_arg = self.on_exchange_recharge
#		self.btnCz.on_lbutton_up_arg = self.on_goto_recharge
#
#	def on_click_get_first(self,btn):
#		s_get_welfare_reward(self.get_reward_index(),"1",btn)
#
#	#def on_click_get_second(self,btn):
#	#	s_get_welfare_reward(self.get_reward_index(),"2",btn)
#
#	def init_item_add(self):
#		import welfare_info
#		for i in range(0,5):
#			info  = welfare_info.NEW_FIRST_RECHARGE_CONTENT[0][i]
#			item_info = self.get_item_info(info)
#			item = getattr(self, "item1%d"%(i+1))
#			item.add_item(item_info)
#			self.btnGet1.on_lbutton_up_arg = self.on_click_get_first
#
#		#for i in range(0,5):
#		#	info  = welfare_info.NEW_FIRST_RECHARGE_CONTENT[1][i]
#		#	item_info = self.get_item_info(info)
#		#	item = getattr(self, "item2%d"%(i+1))
#		#	item.add_item(item_info)
#		#	self.btnGet2.on_lbutton_up_arg = self.on_click_get_second
#
#	def update(self,key,value):
#		import net_data
#		number = int(net_data.hero.get_data(self.get_update_key()))
#		is_can_got1 =  number/10
#		is_can_got2 = number%10
#
#		if is_can_got1 == 1:
#			self.btnGet1.disable(False)
#			self.aniGet1.hide(False)
#			#self.imgMb1.hide(True)
#			#self.imgGot1.hide(True)
#		else:
#			#self.btnGet1.disable(True)
#			self.aniGet1.hide(True)
#			#self.imgMb1.hide(True)
#			#self.imgGot1.hide(True)
#
#		#if is_can_got2 == 1:
#		#	self.btnGet2.disable(False)
#		#	self.aniGet2.hide(False)
#		#	self.imgMb2.hide(True)
#		#	self.imgGot2.hide(True)
#		#else:
#		#	#self.btnGet2.disable(True)
#		#	self.aniGet2.hide(True)
#		#	self.imgMb2.hide(True)
#		#	self.imgGot2.hide(True)
#
#		rw_list = pro = net_data.hero.get_data(self.get_update_list_key())
#		if rw_list.count(1) > 0:
#			self.btnGet1.disable(True)
#			self.btnGet1.set_filename("func/ui_material/shouchong/ylq0.zgp")
#			self.aniGet1.hide(True)
#			#self.imgMb1.hide(False)
#			#self.imgGot1.hide(False)
#			self.btnGet1.hide(True)
#
#		#if rw_list.count(2) > 0:
#		#	self.btnGet2.disable(True)
#		#	self.btnGet2.set_filename("func/ui_material/shouchong/ylq0.zgp")
#		#	self.aniGet2.hide(True)
#		#	self.imgMb2.hide(False)
#		#	self.imgGot2.hide(False)
#		#	self.btnGet2.hide(True)
#		time_list = net_data.hero.get_data(self.get_update_rw_get_key())
#		if time_list == None:
#			return
#
#		duration = time_list[0]
#		if duration >= 48*3600:
#			self.timer.hide(True)
#			self.panel2.set_filename("func/ui_material/shouchong/xinshouchong/new/10bg2.zgp")
#			self.item11.set_coord(202,352)
#			self.item12.set_coord(263,352)
#			self.item13.set_coord(324,352)
#			self.item14.set_coord(384,352)
#			self.item15.set_coord(445,352)
#			self.btnTq.set_coord(501,421)
#			self.btnGet1.set_coord(600,421)
#			self.aniGet1.set_coord(567,392)
#		else:
#			self.timer.set_duration(48*3600-duration)
#		s_refresh_welfare_menu(1)
#
#	def show(self):
#		pass
#
#	def get_reward_index(self):
#		return RTAG.REWARD_NEW_FIRST_RECHARGE
#
#	def on_release(self):
#		self.unreg_update_func()
#		self.timer.pause()

# class CDlgWelfareGrowUp(CDlgRewardPnlBaseBase,ui.CPanel):
#	def init_base(self):
#		self.cur_btn = 2
#		self.init_lv_button()
#		self.init_public()
#		self.reg_update_func()
#		#self.init_lv_button()
#		self.btnBuy.on_lbutton_up_arg = self.on_click_buy_btn
#		self.btnBuy.card_cd = None
#
#	def init_lv_button(self):
#		import welfare_info
#		for i in range(0,11):
#			lv = welfare_info.GROWUP_IMG[i][0]
#			#lv = welfare_info.GROWUP_LV[i]
#			_btn = getattr(self,"btn%d"%(lv))
#			_btn._lv = lv
#			_btn.on_lbutton_up_arg = self.on_click_lv_btn
#			_btn.card_cd = None
#
#		import net_data
#		host = net_data.get_hostid()
#
#		for i in range(0,3):
#			_typebtn = getattr(self,"btnGpType%d"%(i+1))
#			_typebtn._type = i+1
#			_typebtn.on_lbutton_up_arg = self.on_click_type_btn
#			_typebtn.card_cd = ""
#
#			if (i == 0 or i == 2) and host >= 1250:
#				_typebtn.hide(True)
#
#		if host >= net_data.get_new_hostid():
#		#if host >= 1260:
#			for i in range(1,4):
#				_msg = getattr(self,"msg%d"%(i+1))
#				_msg.hide(True)
#			self.imgBanner.set_filename("func/ui_material/gongyongyoude/tiao.zgp")
#			_typebtn = getattr(self,"btnGpType%d"%(2))
#			_typebtn.hide(True)
#
#			x, y = self.btnBuy.get_coord()
#			x = x + 5
#			y = y + 48
#			self.btnBuy.set_coord(x, y)
#
#	def update(self,key,value):
#		import net_data
#		is_buy = net_data.hero.get_data(self.get_update_key())
#		list = pro = net_data.hero.get_data(self.get_update_list_key())
#		rw_list = net_data.hero.get_data(self.get_update_rw_get_key())
#		self.update_buy_btn(is_buy)
#		self.update_lv_btn(list,rw_list)
#		s_refresh_welfare_menu(1)
#		self.reg_update_func()
#
#	def update_buy_btn(self,is_buy):
#		if is_buy == 1:
#			self.btnBuy.disable(True)
#		elif is_buy == 0:
#			self.btnBuy.disable(False)
#
#	def update_lv_btn(self,list,rw_list):
#		import welfare_info
#		for i in range(0,11):
#			lv = welfare_info.GROWUP_IMG[i][0]
#			#lv = welfare_info.GROWUP_LV[i]
#			_btn = getattr(self,"btn%d"%(lv))
#			_img = getattr(self,"img%d"%(lv))
#			jp = welfare_info.GROWUP_IMG[i][self.cur_btn]
#			_img.set_filename("func/ui_material/chengzhang/%d.zgp"%jp)
#			if list.count(lv) > 0:
#				_btn.disable(False)
#			else:
#				_btn.disable(True)
#
#			if rw_list.count(lv) > 0:
#				_btn.hide(True)
#				_img.hide(True)
#			else:
#				_btn.hide(False)
#				_img.hide(False)
#
#		import welfare_info
#		self.imgBanner.set_filename(welfare_info.GP_BANNER_IMG[(self.cur_btn-1)])
#
#		import net_data
#		host = net_data.get_hostid()
#		if host >= net_data.get_new_hostid():
#		#if host >= 1260:
#			self.imgBanner.set_filename("func/ui_material/gongyongyoude/tiao.zgp")
#
#	def on_click_lv_btn(self,btn):
#		import net_data
#		import welfare_info
#
#		#vip_lv = net_data.hero.get_data("VipLv") or 0
#		#import dlg_msg
#		#def get_ok_func(is_ok,args):
#		#	if is_ok:
#		#		s_get_welfare_reward(self.get_reward_index(),str(btn._lv),btn)
#		#
#		#if welfare_info.GROWUP_VIP.has_key(btn._lv) and vip_lv < welfare_info.GROWUP_VIP[btn._lv]:
#		#	dlg_msg.show(txtMsg="到达#GVIP"+str(welfare_info.GROWUP_VIP[btn._lv])+"#n后可领取额外奖励，您当前是#GVIP"+str(vip_lv)+#"#n#r领取金票之后将放弃当前的vip额外奖励。您确定要领取该等级的奖励吗？",call_back_func=get_ok_func,args=())
#		#else:
#		s_get_welfare_reward(self.get_reward_index(),str(btn._lv),btn)
#
#
#	def on_click_type_btn(self,btn):
#		self.unreg_update_func()
#		self.cur_btn = btn._type
#		self.reg_update_func()
#		s_get_welfare_reward(self.get_reward_index(),"",btn)
#		import welfare_info
#		self.imgBanner.set_filename(welfare_info.GP_BANNER_IMG[(self.cur_btn-1)])
#
#	def on_click_buy_btn(self,btn):
#		import dlg_msg
#		def buy_ok_func(is_ok,args):
#			if is_ok:
#				import hjrpc
#				s_get_welfare_reward(self.get_reward_index(),"Gp"+str(self.get_reward_index()),btn)
#
#		ds_str = ""
#		if self.cur_btn == 1:
#			ds_str = 998
#		if self.cur_btn == 2:
#			ds_str = 1980
#		if self.cur_btn == 3:
#			ds_str = 2980
#
#		dlg_msg.show(txtMsg="种植成长树需要消耗"+str(ds_str)+"元宝,您确认要种植吗?",call_back_func=buy_ok_func,args=())
#
#	def get_reward_index(self):
#		if self.cur_btn == 1:
#			return RTAG.REWARD_GROWUP1
#		elif self.cur_btn == 2:
#			return RTAG.REWARD_GROWUP2
#		elif self.cur_btn == 3:
#			return RTAG.REWARD_GROWUP3
#
#	def show(self):
#		pass

# class CDlgWelfareMediaMulti(CDlgRewardPnlBaseBase,ui.CPanel):
#	def init_base(self):
#		self._timer_id = None
#		import reward_info
#		#reward_info.REWARD_ITEMS[self.get_reward_index()] = []
#		items = reward_info.REWARD_ITEMS[self.get_reward_index()]
#		import welfare_info
#		li = welfare_info.RewardExtractItems
#		for item in li:
#			items.append((item[0],item[1]))
#
#		for i in range(0,len(items)):
#			if hasattr(self,"grid%d"%(i+1)):
#				grid = getattr(self,"grid%d"%(i+1))
#				_item_info = self.get_item_info(items[i])
#				grid.add_item(_item_info)
#				grid.set_filename("func/ui_material/choujiang/xkan.zgp")
#
#		text = "激活码说明：#r1、首次集齐任意#G3种激活码#n即可领取#G绝版称号#n和#G激活码礼包#n"\
#			"#r2、每天#G集齐3种激活码#n可抽取1次#G激活码大奖#n。#r3、#Y$html(更多活动介绍)$查看此处"
#		import richformat
#		#richformat.set_richlabel_text_ex(self.desc,text)
#		#self.desc.set_text(text)
#		self.init_public()
#		self.reg_update_func()
#		self.bind_event()
#		self.aniGet.hide(1)
#		self.aniExtract.hide(1)
#
#	def bind_event(self):
#		for i in range(0,4):
#			#_btn = getattr(self,"btnActive%d"%(i+1))
#			#_btn._index = i+1
#			#_btn.on_lbutton_up_arg = self.on_active_up
#
#			_btn = getattr(self,"btnGo%d"%(i+1))
#			_btn._index = i+1
#			_btn.on_lbutton_up_arg = self.on_go_up
#
#		self.btnGet.card_cd = None
#		self.btnGet.on_lbutton_up_arg = self.on_get_up
#		self.btnExtract.card_cd = None
#		self.btnExtract.on_lbutton_up_arg = self.on_extract_up
#		self.btnExtract.on_mouse_enter = self.on_extract_mouse_enter
#		self.btnExtract.on_mouse_leave = self.on_extract_mouse_leave
#		return
#
#		# import time, datetime, math
#		# dateC = datetime.datetime(2016,7,16,23,59)
#		# timestamp=time.mktime(dateC.timetuple())
#
#		# left   =	math.max(0,  timestamp - time.time() )
#		# day    =    math.floor(left/86400)
#		# hour   =	math.floor((left % 86400)/3600)
#		# minute =    math.max(1, math.ceil(((left%86400)%3600)/60))
#		# self.lblLeftTime.set_text("剩余%d天 %d时 %d分"%(day,hour,minute))
#
#	def update(self,key,value):
#		import net_data
#		state = net_data.hero.get_data(self.get_update_key())
#		if state!=None:
#			amount = 0
#			#for i in range(0,5):
#			#	_btn = getattr(self,"btnActive%d"%(i+1))
#			#	st = state & (1<<i)
#			#	_btn.disable(st)
#			#	if st:
#			#		amount +=1
#			#		_btn.set_text("已激活")
#			#	else:
#			#		_btn.set_text("激活")
#
#			for i in range(0,4):
#				_imgTag = getattr(self,"tagGot%d"%(i+1))
#				_btnGo = getattr(self,"btnGo%d"%(i+1))
#				st = state & (1<<i)
#				if st:
#					#已经激活了
#					_imgTag.set_filename("check/11/check.zgp")
#					_btnGo.hide(1)
#					_imgTag.hide(0)
#				else:
#					#还没有激活
#					_imgTag.set_filename("check/11/uncheck.zgp")
#					_btnGo.hide(0)
#					_imgTag.hide(1)
#
#			hasget = state & (1<<4)
#			fit = state & (1<<5)
#			if hasget:
#				self.btnGet.set_text("已领取")
#				self.aniGet.hide(1)
#			else:
#				self.btnGet.set_text("领取礼包")
#				self.aniGet.hide(0)
#
#			if hasget or not fit:
#				self.btnGet.disable(1)
#				self.aniGet.hide(1)
#			else:
#				self.btnGet.disable(0)
#				self.aniGet.hide(0)
#
#			st = state & (1<<6)
#			if st:
#				self.btnExtract.set_filename("func/ui_material/choujiang/yichou.zgp")
#				self.aniExtract.hide(1)
#			else:
#				self.btnExtract.set_filename("func/ui_material/choujiang/choujiangj.zgp")
#
#			self.btnExtract.disable(st)
#			if fit:
#				self.aniExtract.hide(st)
#		else:
#			#for i in range(0,5):
#			#	_btn = getattr(self,"btnActive%d"%(i+1))
#			#	_btn.disable(1)
#			self.btnGet.disable(1)
#			self.aniGet.hide(1)
#			self.btnExtract.disable(1)
#			self.aniExtract.hide(1)
#
#		s_refresh_welfare_menu(1)
#
#	def update_ani(self):
#		for i in range(0,12):
#
#			grid = getattr(self,"grid%d"%(i+1))
#			grid._ani_effect.hide(i != self._show_index)
#
#		import welfare_info
#		li = welfare_info.RewardExtractItems
#		item = li[self._show_index]
#
#		import item_info
#		shape = item_info.item_info[item[0]][item_info.SHAPE]
#		self.icon.set_filename("icon/item/100x100/%04d.zgp"%shape)
#
#
#	def on_timer(self):
#		if not self.is_valid():
#			if self._timer_id!=None:
#				import hjrpc,pto_comm
#				#hjrpc.server.s_get_operate_reward(pto_comm.cREWARD_TAG.REWARD_MEDIA_MULTI_EXTRACT,"")
#				s_get_welfare_reward(pto_comm.cREWARD_TAG.REWARD_MEDIA_MULTI_EXTRACT,"",self.btnExtract)
#				self._timer_id = None
#			return 0
#		self._all_elips_time +=1
#		self._elips_time +=1
#
#
#		#做个速度曲线公式吧
#		speed = 0.0002*self._all_elips_time*self._all_elips_time-0.05*self._all_elips_time+2
#		self._speed = speed
#
#		self._speed = min(self._speed,30)
#		self._speed = max(self._speed,0.01)
#
#		if self._elips_time>self._speed:
#			self._elips_time = 0
#			self._show_index += 1
#			if self._show_index>11:
#				self._show_index = 0
#
#		self.update_ani()
#
#		if self._all_elips_time>280 and self._show_index == self._open_result_index:
#			import hjrpc,pto_comm
#			#hjrpc.server.s_get_operate_reward(pto_comm.cREWARD_TAG.REWARD_MEDIA_MULTI_EXTRACT,"")
#			s_get_welfare_reward(pto_comm.cREWARD_TAG.REWARD_MEDIA_MULTI_EXTRACT,"",self.btnExtract)
#			self._timer_id = None
#			self.btnExtract.disable(1)
#			return 0
#
#	def show(self):
#		pass
#
#	def get_reward_index(self):
#		return RTAG.REWARD_MEDIA_MULTI
#
#	#def on_active_up(self,btn):
#	#	btn._index
#	#	edit = getattr(self,"edit%d"%btn._index)
#	#	text = edit.get_text()
#	#	if text!="":
#	#		import pto_comm
#	#		import hjrpc
#	#		v = getattr(pto_comm.cREWARD_TAG,"REWARD_MEDIA_MULTI_%d"%btn._index)
#	#		#hjrpc.server.s_get_operate_reward(v,text)
#	#		s_get_operate_reward(v,text)
#
#	def on_go_up(self,btn):
#		links = [
#			"http://ka.duowan.com/v5/spc?id=160",
#			"http://hao.17173.com/gift-info-29004.html",
#			"http://ka.sina.com.cn/18439",
#			"http://card.yzz.cn/fahao-3837.html",
#			"http://ka.duowan.com/11181.html",
#		]
#		import webbrowser
#		webbrowser.open(links[btn._index-1])
#
#
#	def on_get_up(self,btn):
#		import hjrpc,pto_comm
#		#hjrpc.server.s_get_operate_reward(pto_comm.cREWARD_TAG.REWARD_MEDIA_MULTI_FIRST,"")
#		s_get_welfare_reward(pto_comm.cREWARD_TAG.REWARD_MEDIA_MULTI_FIRST,"",btn)
#
#	def on_extract_up(self,btn):
#		import net_data
#		state = net_data.hero.get_data(self.get_update_key())
#
#		if state==None:return
#
#		#self.c_extract_rt(5583)
#		#return
#		amount = 0
#		for i in range(0,4):
#			st = state & (1<<i)
#			if st:
#				amount +=1
#		if amount<4:
#			import msgbox
#			msgbox.show("您当前没有获得抽奖的资格，请激活4个激活码之后再来进行抽奖。")
#			return
#
#		self.btnExtract.disable(1)
#
#		import hjrpc,pto_comm
#		#hjrpc.server.s_get_operate_reward(pto_comm.cREWARD_TAG.REWARD_MEDIA_MULTI_PRE,"")
#		s_get_welfare_reward(pto_comm.cREWARD_TAG.REWARD_MEDIA_MULTI_PRE,"",btn)
#
#	def on_extract_mouse_enter(self):
#		self.btnExtract.set_filename("func/ui_material/choujiang/choujianghh.zgp")
#
#	def on_extract_mouse_leave(self):
#		self.btnExtract.set_filename("func/ui_material/choujiang/choujiangj.zgp")
#
#	def c_extract_rt(self,item_type):
#		self._open_result_index = 0
#		import reward_info
#		temps = reward_info.REWARD_ITEMS[self.get_reward_index()]
#		items = []
#		for item in temps:
#			items.append(item[0])
#		if item_type in items:
#			self._open_result_index = items.index(item_type)
#		if self._timer_id==None:
#			self._all_elips_time = 0
#			self._elips_time = 0
#			self._speed = 2
#			self._show_index = 0
#			import timer
#			for i in range(0,12):
#				grid = getattr(self,"grid%d"%(i+1))
#				#grid.play_item_use_effect("ani/item_effect/ani_clock.zgp",loop=True)
#				grid.play_item_use_effect("ani/item_effect/item_use3.zgp",loop=True)
#				grid._ani_effect.set_frame(1)
#				grid._ani_effect.hide(1)
#			self._timer_id = timer.start_timer(1,self.on_timer)

# class CDlgWelfareLogin(CDlgRewardPnlBaseBase,ui.CPanel):
#	def init_base(self):
#		self.init_public()
#		self.bind_event()
#		self.reg_update_func()
#
#	def get_ui_config(self):
#		import welfare_info
#		return welfare_info.LOGIN_ITEMS
#
#	def bind_event(self):
#		ui_config = self.get_ui_config()
#		for i in range(1, 8):
#			_panel = getattr(self,"panel%d"%(i))
#			#_panel.signCount.set_text(i)
#			#_panel.ani.hide(True)
#			_panel.btnSign.on_lbutton_up_arg = self.on_click_sign_btn
#			_panel.btnSign.card_cd = None
#			_panel.imgSign.hide(True)
#			#_panel.ani.message = _panel.btnSign.message
#			grid = 1
#			for info in ui_config[i-1]:
#				item_info = self.get_item_info(info)
#				item = getattr(_panel, "item%d"%grid)
#				item.add_item(item_info)
#				grid += 1
#			for i in range(grid,6):
#				item = getattr(_panel, "item%d"%i)
#				item.hide(True)
#
#
#	def update(self,key,value):
#		import net_data
#		info_list = net_data.hero.get_data(self.get_update_list_key())
#		if info_list == None:
#			return
#		login_total, login_today = info_list[0], info_list[1]
#		for i in range(1, 8):
#			_panel = getattr(self,"panel%d"%(i))
#			if i <= login_total:
#				_panel.btnSign.hide(True)
#				_panel.imgSign.hide(False)
#				#_panel.ani.hide(True)
#			if i == (login_total + 1 ) and login_today == 0:
#				_panel.btnSign.hide(False)
#				_panel.imgSign.hide(True)
#				#_panel.ani.hide(False)
#
#	def show(self):
#		import hjrpc
#		hjrpc.server.s_get_welfare_reward_info(self.get_reward_index())
#
#	def on_click_sign_btn(self, btn):
#		import net_data
#		info_list = net_data.hero.get_data(self.get_update_list_key())
#		if info_list == None:
#			return
#		login_total, login_today = info_list[0], info_list[1]
#		if login_today > 0:
#			import msgbox
#			msgbox.show("今天已经签到过了。")
#			return
#
#		s_get_welfare_reward(self.get_reward_index(),"",btn)
#		s_refresh_welfare_menu(1)
#
#	def get_reward_index(self):
#		return RTAG.REWARD_LOGIN

# class CDlgWelfareGuoqing(CDlgWelfareLogin):
#	def get_reward_index(self):
#		return RTAG.REWARD_GUOQING
#
#	def get_ui_config(self):
#		import welfare_info
#		return welfare_info.GUOQING_ITEMS

# class CDlgWelfareMedia(CDlgRewardPnlBaseBase,ui.CPanel):
#	def init_base(self):
#		self.init_public()
#		self.reg_update_func()
#		self.edit.set_max_count(32)
#		self.btnGet.card_cd = 0
#		self.btnGet.on_lbutton_up_arg = self.on_click_get_btn
#		#self.init_msg()
#		self.edit.on_msg_char = self.on_msg_char
#
#		import common
#		if common.TAG_17173:
#			self._label.set_text("特权礼包CDKey")
#			self.btnGet.set_filename("func/ui_material/b6/btn_duihuan.zgp")
#
#	def on_msg_char(self):
#		import welfare_info
#		text = self.edit.get_text()
#		length = len(text)
#		if length == 24:
#			text = text[4:]
#		key2item = welfare_info.REWARD_KEY_PREFIX_2_ITEMS
#		prefix = text[:2]
#		prefix = prefix.upper()
#		prefix4 = text[:3]
#
#		items = None
#		if length>4 and key2item.has_key(prefix4):
#			items = key2item[prefix4]
#		elif length>2 and key2item.has_key(prefix):
#			items = key2item[prefix]
#		elif key2item.has_key(text.upper()):
#			items = key2item[text.upper()]
#		if items:
#			for i,item in enumerate(items):
#				_item_info = self.get_item_info(item)
#				_item_info["grid"] = i+1
#				self.frame.add_item(_item_info)
#		else:
#			self.frame.clear()
#
#	def update(self,key,value):
#		self.btnGet.disable(False)
#		s_refresh_welfare_menu(1)
#
#	def init_msg(self):
#		import richformat
#		import welfare_res
#		richformat.set_all_link_color("#c0066ff")
#		richformat.set_richlabel_text_ex(self.desc,welfare_res.media_msg)
#		#self.desc.hide(True)
#
#	def on_click_get_btn(self,btn):
#		text = self.edit.get_text()
#		if text!="":
#			#import hjrpc
#			#hjrpc.server.s_get_welfare_reward(self.get_reward_index(),text)
#			s_get_welfare_reward(self.get_reward_index(),text,btn)
#
#	def get_reward_index(self):
#		return RTAG.REWARD_MEDIA
#
#	def show(self):
#		pass

# class CDlgWelfareNewYear(CDlgRewardPnlBaseBase,ui.CPanel):
#	def init_base(self):
#		self.bind_event()
#		self.init_public()
#		self.reg_update_func()
#		self.set_item_info()
#	def set_item_info(self):
#		import welfare_info
#		for i in range(3):
#			for j in range(6):
#				grid = getattr(self,"item%d_%d"%(i,j))
#				item_id = welfare_info.NEWYEARREWOARD[i][j][0]
#				item_num = welfare_info.NEWYEARREWOARD[i][j][1]
#				grid.add_item_by_id(item_id,item_num)
#
#
#	def update(self,key,value):
#		import net_data
#		data = net_data.hero.get_data(self.get_update_list_key())
#		for i in range(len(data)):
#			btn = getattr(self,"btnGet%d"%(i+1))
#			btn_hide = getattr(self,"ylq%d"%(i+1))
#			if data[i]<1:
#				btn.disable(0)
#				btn_hide.hide(1)
#			else:
#				btn.disable(1)
#				btn_hide.hide(0)
#		return
#
#	def bind_event(self):
#		self.btnGet1.on_lbutton_up = lambda:self.on_click_get_btn(1)
#		self.btnGet2.on_lbutton_up = lambda:self.on_click_get_btn(2)
#		self.btnGet3.on_lbutton_up = lambda:self.on_click_get_btn(3)
#
#	def on_click_get_btn(self,index):
#		import hjrpc
#		hjrpc.server.s_get_welfare_reward(RTAG.REWARD_NEW_YEAR,str(index))
#		return
#
#	def get_reward_index(self):
#		return RTAG.REWARD_NEW_YEAR
#
#	def show(self):
#		pass


########################################################################################################################
class CDlgWelfare(ui.CImgDialog):
	def init_base(self, default_type,is_new):
		ui.CImgDialog.init_base(self)
		self._type = default_type
		self._is_new = is_new#是否是开服活动
		self._inited = None
		self._config = None
		self._panel_list = {}
		#self.init_ui()
		self._cur_reward_index = None
		# self.change_type(default_type)
		# self.init_ani()
		self.unreg_update_func()
		self.reg_update_func()
		# self.imgTitle.disable(0)
		# self.imgTitle.message = self.message
		s_refresh_welfare_menu(1)
		import hjrpc
		self._btnClose.set_coord(1037, -6)
		self.CLabelTitle.set_text("挑战活动" if self._is_new else "福利中心")
		self.set_filename(XINFU_BG if self._is_new else WELFARE_BG)
		if not self._is_new:
			hjrpc.server.s_open_branch_target(0)
			hjrpc.server.s_get_welfare_reward_info(RTAG.REWARD_UPGRADE)
		self.black_box()

	def reg_update_func(self):
		import net_data
		net_data.hero.reg_update_hero_func("WelfareRefreshInfo", self.update_ani_show)
		net_data.hero.reg_update_hero_func("WelfareRefreshInfo2", self.update_ani_show2)

	def unreg_update_func(self):
		import net_data
		net_data.hero.unreg_update_hero_func("WelfareRefreshInfo", self.update_ani_show)
		net_data.hero.unreg_update_hero_func("WelfareRefreshInfo2", self.update_ani_show2)

	def get_menu_size(self):
		w, h = self.lstMenu.get_size()
		return w,71

	def get_menu_coord(self):
		x, y = self.lstMenu.get_coord()
		return x, y

	def init_ui(self):
		if self._inited:
			return
		import net_data
		WelfareRefreshInfo = net_data.hero.get_data("WelfareRefreshInfo")
		if WelfareRefreshInfo == None:
			return

		hide_tags = []
		host = net_data.get_hostid()
		for info in WelfareRefreshInfo:
			# if info['show'] == 0:
			if info['show'] == 0:
				# if info['show'] == 0 or (host >= 1260 and info['tag'] == RTAG.REWARD_FIRST_RECHARGE) or (host < 1260 and info['tag'] == RTAG.REWARD_NEW_FIRST_RECHARGE):
				hide_tags.append(info['tag'])
		if common.SHENCHA_TAG:
			for hide_tag in [RTAG.REWARD_LIMIT, RTAG.REWARD_BONUS, RTAG.REWARD_NEW_FIRST_RECHARGE2,RTAG.REWARD_FIRST_TEST]:
				hide_tags.append(hide_tag)
		if host >= net_data.get_new_hostid():
			# if host >= 1260:
			hide_tags.append(RTAG.REWARD_MEDIA)
		import time,welfare_info
		#_time = get_servertime()
		#_time2 = time.mktime([2017, 4, 12, 23, 59, 59, 0, 0, 0])
		# kfxl_time_s = time.mktime(welfare_info.KFXL_START_TIME)
		# kfxl_time_e = time.mktime(welfare_info.KFXL_END_TIME)
		# if _time2 < _time:
		#	hide_tags.append(RTAG.REWARD_NEW_YEAR)
		# if _time < kfxl_time_s or _time > kfxl_time_e:
		#	hide_tags.append(RTAG.REWARD_GIFTPACK)

		self._config = {}
		i=0
		for Wel in gWelfares:
			_tag = Wel[3]
			if _tag not in hide_tags:
				if (self._is_new and _tag in XINFU_PANEL_LIST) or  (not self._is_new and _tag not in XINFU_PANEL_LIST):
					if i == 0 and self._type == 0:
						self._type = _tag
						i+=1
					self._config[_tag]=Wel
		#print sorted(self._config.iteritems(),key=lambda e:e[0])
		self.lstMenu.disable_child(1)
		self.lstMenu.set_template("pnl_welfare_menu")
		self.lstMenu.hide_image(True)
		self.lstMenu.init()

		self.update_menu()


		for tag, wel in sorted(self._config.iteritems(),key=lambda e:e[0]):
			if tag == self._type:
				self.change_type(tag)
		self._inited = True

	def update_menu(self):
		self.lstMenu.clear()
		self.lstMenu.add_fixed_count_items(len(self._config))
		self.lstMenu.set_default_event(self.on_click_type)
		i = 0
		w, h = self.get_menu_size()
		for tag,Wel in sorted(self._config.iteritems(),key=lambda e:e[0]):
			item_display = self.lstMenu.get_item(i)
			item_display.get_size = self.get_menu_size
			item_display.set_rect(w, h)
			item_display.btnMenu.set_text(Wel[0])
			item_display.btnMenu.set_color(0x354975)
			item_display.btnMenu.set_font(common.FONT_SONGTI_20)
			item_display._tag = Wel[3]
			item_display.aniMenu.hide(True)
			item_display.aniMenu.disable(1)
			setattr(self, "btnReward%d" % (i + 1), item_display.btnMenu)
			setattr(self, "ani%d" % (i + 1), item_display.aniMenu)
			i += 1
		self.lstMenu.update()
		item=self.get_welfare_by_tag(self._type)
		if item:
			item.btnMenu.set_selected(True,1)

		#盖世端游回归
		self._create_panel(RTAG.REWARD_GSHX_CHECKIN)
		hjrpc.server.s_get_welfare_reward_info(RTAG.REWARD_GSHX_CHECKIN)

	def get_welfare_by_tag(self, type):
		for item in self.lstMenu.get_items():
			if item._tag == type:
				return item

	def get_panel_by_tag(self, type):
		for item in self.lstMenu.get_items():
			if item._tag == type and self._panel_list.has_key(item._tag):
				return self._panel_list[item._tag]

	def change_type(self, type):
		self._type = type
		self._create_panel(type)
		for item in self.lstMenu.get_items():
			i = item._tag
			if self._panel_list.has_key(i):
				if type == i:
					self._panel_list[i].hide(False)
					self._panel_list[i].show()
					self._cur_reward_index = self._panel_list[i].get_reward_index()
				else:
					self._panel_list[i].hide(True)


	def on_click_type(self, btn):
		self.change_type(btn._tag)
		for item in self.lstMenu.get_items():
			item.btnMenu.set_selected(False, 0)
		btn.btnMenu.set_selected(True, 1)

	def _create_panel(self, type):
		import net_data
		# host = net_data.get_hostid()
		if not self._panel_list.has_key(type) and self._config.has_key(type):
			import ui_template
			ui_template_address = self._config[type][1]
			if self._config[type][3] == RTAG.REWARD_FIRST_TEST:
				# if host >= 1260 and self._config[type][3] == RTAG.REWARD_FIRST_TEST:
				ui_template_address = "pnl_welfare_card_new"
			self._panel_list[type] = ui_template.create_customize(self, ui_template_address, self._config[type][2])

			x,y=285, 65
			if hasattr(self._panel_list[type],"get_xy"):
				x, y=self._panel_list[type].get_xy()
			self._panel_list[type].set_coord(x, y)

	# 这个是外部数据变化，比如升级了，而这两个界面正在打开， 这时候更新界面
	def update_ani_show2(self, key, value):
		if self._cur_reward_index != None and (
				self._cur_reward_index == RTAG.REWARD_FIRST_RECHARGE or self._cur_reward_index == RTAG.REWARD_UPGRADE):
			import hjrpc
			hjrpc.server.s_get_welfare_reward_info(self._cur_reward_index)

		# 这个函数是为了处理会幸运抽奖的特殊处理
		if self.is_valid():
			self.change_item_show_by_tag(RTAG.REWARD_LUCKY_PRIZE)

	def update_ani_show(self, key, value):
		if not self.is_valid() or value == None:
			return
		self.init_ui()
		# import net_data
		# rw_list = net_data.hero.get_data("WelfareRewardGetInfo%d" % RTAG.REWARD_UPGRADE)
		# if rw_list and len(rw_list) == 18 and self._config[3][3] == RTAG.REWARD_UPGRADE:
		#	del self._config[3]
		#	self.update_menu()
		#	self.on_click_type(panel.lstMenu.get_item(0))
		for info in value:
			for item in self.lstMenu.get_items():
				if item._tag == info['tag']:
					item.aniMenu.hide(info['ani'])

	def hide_by_tag(self, tag):
		for item in self.lstMenu.get_items():
			if item._tag == tag:
				self.lstMenu.hide_item(item)
				break

		for k, v in self._panel_list.items():
			if v.get_reward_index() == tag:
				v.hide(True)

	def change_item_show_by_tag(self, tag):
		import net_data
		WelfareRefreshInfo = net_data.hero.get_data("WelfareRefreshInfo2")
		if WelfareRefreshInfo == None:
			return

		is_show = None
		is_ani = None
		for info in WelfareRefreshInfo:
			if info['tag'] == tag:
				is_show = info['show']
				is_ani = info['ani']

		if is_show == None:
			return

		is_exsit = None
		for item in self.lstMenu.get_items():
			if item._tag == tag:
				is_exsit = 1

		if is_exsit == None:
			if is_show == 1:
				for Wel in gWelfares:
					if Wel[3] == tag:
						self._config[tag]=Wel
					self.lstMenu.add_fixed_count_items(len(self._config))
					i = 0
					w, h = self.get_menu_size()
					for _tag,Wel in self._config.iteritems():
						if _tag == tag:
							item_display = self.lstMenu.get_item(i)
							item_display.get_size = self.get_menu_size
							item_display.set_rect(w, h)
							item_display.btnMenu.set_filename(Wel[0])
							item_display._tag = Wel[3]
							item_display.aniMenu.hide(is_ani)
							setattr(self, "btnReward%d" % (i + 1), item_display.btnMenu)
							setattr(self, "ani%d" % (i + 1), item_display.aniMenu)
						i += 1
					self.lstMenu.update()
			else:
				for item in self.lstMenu.get_items():
					if item._tag == tag:
						self.lstMenu.hide_item(item)
		else:
			panel = get_panel_bytag(tag)
			if panel and hasattr(panel,"ani_btn"):
				panel.ani_btn.hide(is_ani)

			for item in self.lstMenu.get_items():
				if item._tag == tag:
					if not is_show:
						self.lstMenu.hide_item(item)

	def after_hide(self,b):
		self.unreg_update_func()
		for k,v in self._panel_list.iteritems():
			if hasattr(v,"stop_timer"):
				v.stop_timer()

	def after_release(self):
		self.unreg_update_func()
		for k,v in self._panel_list.iteritems():
			if hasattr(v,"stop_timer"):
				v.stop_timer()

def get_servertime():
	import systemprocess
	servertime = systemprocess.get_servertime()
	if servertime == 0:
		import time
		return time.time()
	return servertime

def get_panel_bytag(tag):
	global panel
	if panel == None:
		return
	for _tag, wel in panel._config.iteritems():
		if wel[3] == tag:
			return panel._panel_list.get(_tag)

def get_tags():
	global gWelfares
	tags = []
	import net_data
	host = net_data.get_hostid()
	for wel in gWelfares:
		if (host >= net_data.get_new_hostid() and wel[3] == RTAG.REWARD_FIRST_RECHARGE) or (host >= net_data.get_new_hostid() and wel[3] == RTAG.REWARD_MEDIA):
		#if (host >= 1260 and wel[3] == RTAG.REWARD_FIRST_RECHARGE) or (host >= 1260 and wel[3] == RTAG.REWARD_MEDIA):
			continue
		
		if host >= net_data.get_new_hostid() and (wel[3] == RTAG.REWARD_NEW_FIRST_RECHARGE or wel[3] == RTAG.REWARD_NEW_FIRST_RECHARGE2):
		#if host < 1260 and (wel[3] == RTAG.REWARD_NEW_FIRST_RECHARGE or wel[3] == RTAG.REWARD_NEW_FIRST_RECHARGE2):
			continue
		tags.append(wel[3])
	return tags

def c_media_multi_result(rt):
	global panel
	if panel and panel.is_valid():
		if panel._panel_list.has_key(MEDIA_MULTI_PAGE_TYPE):
			p = panel._panel_list[MEDIA_MULTI_PAGE_TYPE]
			p.c_extract_rt(rt)
			
def show(type = 0,is_new=0):
	global panel
	global panel1
	global panel2
	if not is_new:
		_panel = panel1
	else:
		_panel = panel2
	def create():
		import ui_template,game
		parent= game.GamePanel
		return ui_template.create_customize(parent,"ui_welfare_m", CDlgWelfare,init_base_arg = (type,is_new))
	import udialog
	if _panel != None and _panel.is_valid():
		b = _panel.is_hide()
		_panel.hide(not b)
		if not b:
			_panel.release()
			_panel = None
			return
	_panel = udialog.special_show(_panel,create)
	panel = _panel
	if not is_new:
		panel1 = panel
	else:
		panel2 = panel
	return panel

def get_item_info(info):
	if type(info) == type(()) or type(info) == type([]) :
		item_type, amount = info
	else:
		item_type = info
		amount = 1

	import item_info
	shape = item_info.item_info[item_type][item_info.SHAPE]

	_item_info = {}
	_item_info["id"] = 0
	_item_info["lock"] = False
	_item_info["type"] = item_type
	_item_info["shape"] = shape
	_item_info["amount"] = amount
	return _item_info

def update_world_boss(info):
	global panel2
	if not panel2:
		return
	_panel=panel2.get_panel_by_tag(RTAG.REWARD_WORLDBOSS)
	if not _panel:
		return
	_panel.update_info(info)

def update_card(card_type,cost):
	global panel1
	if not panel1:
		return
	_panel=panel1.get_panel_by_tag(RTAG.REWARD_FIRST_TEST)
	if not _panel:
		return
	_panel.update_card(card_type,cost)

gWelfares = [
	#("超值月卡", "pnl_welfare_card_new", CDlgWelfareCard, RTAG.REWARD_FIRST_TEST),
	("签到有礼","pnl_welfare_sign_part",	CDlgWelfareSign, RTAG.REWARD_EVERYDAY),
	#("首充福利", "pnl_shouchong", CDlgWelfareNewRecharge2, RTAG.REWARD_NEW_FIRST_RECHARGE2),
	#("升级礼包", "pnl_welfare_upgrade", CDlgWelfareUpgrade, RTAG.REWARD_UPGRADE),
	#("消费送礼", "pnl_welfare_consume",CDlgWelfareConsume,RTAG.REWARD_CONSUME),
	("开服献礼", "ui_kfxl",CDlgWelfareGiftPack,RTAG.REWARD_GIFTPACK),
	#("天降宝箱", "pnl_new_server_box",CDlgServerBox,RTAG.REWARD_WORLDBOX),
	#("幸运抽奖", "ui_xycj", CDlgWelfareLucky, RTAG.REWARD_LUCKY_PRIZE),
	("江湖之路", "ui_main_final_target2",CDlgBranchTarget,RTAG.REWARD_JHZL),
	#("聚宝盆", "pnl_treasure_bowl",CDlgJvBaoPen,RTAG.REWARD_NEW_JVBAOPEN),
	#("限时礼包", "ui_xiangoulibao",CDlgLimitTime,RTAG.REWARD_LIMIT),
	#("琅琊榜", "pnl_new_server_top",CDlgServerTop,RTAG.REWARD_LANGYABANG),
	("比武大会", "pnl_new_server_boss",CDlgServerBoss,RTAG.REWARD_WORLDBOSS),
	("限时挑战", "ui_time_limit_boss",CDlgLimitBoss,RTAG.REWARD_LIMITBOSS),
	#("开服礼包", "ui_kaifulibao",CDlgKaifuGiftPack,RTAG.REWARD_KAIFUGIFTPACK),
	("回归福利", "pnl_welfare_sign_part",CDlgGshxCheckIn,RTAG.REWARD_GSHX_CHECKIN),
	#("内测福利", "pnl_bonus_reward",CDlgServerBonus,RTAG.REWARD_BONUS),
	("好友招募", "ui_frd_recruit",CDlgFriendRecruit,RTAG.REWARD_RECRUIT),
	#("mobile/ui_material/biaoqian/btn_sc.zgp", "pnl_welfare_first_charge", CDlgWelfareFirstCharge, RTAG.REWARD_FIRST_RECHARGE)
	#("func/ui_material/shouchong/xinshouchong/sc0.zgp", "pnl_new_recharge_menu", CDlgWelfareNewRecharge, RTAG.REWARD_NEW_FIRST_RECHARGE),
	#("func/ui_material/chengzhang/czjh.zgp", "pnl_welfare_growup", CDlgWelfareGrowUp, RTAG.REWARD_GROWUP1),
	#("func/ui_material/choujiang/sjcdj.zgp", "pnl_reward_media_multi", CDlgWelfareMediaMulti, RTAG.REWARD_MEDIA_MULTI),
	# ("func/ui_material/b2/dliyhl.zgp", "pnl_welfare_login", CDlgWelfareLogin, RTAG.REWARD_LOGIN),
	# ("func/ui_material/gongyongyoude/btn_dengludali.zgp", "pnl_welfare_guoqing", CDlgWelfareGuoqing, RTAG.REWARD_GUOQING),
	# #("func/ui_material/gongyongyoude/btn_cdkey.zgp", "pnl_welfare_media", CDlgWelfareMedia, RTAG.REWARD_MEDIA),

	# ("func/ui_material/yingchunlibao/yclb.zgp", "pnl_welfare_new_year", CDlgWelfareNewYear, RTAG.REWARD_NEW_YEAR),
]
XINFU_PANEL_LIST=(RTAG.REWARD_WORLDBOSS,RTAG.REWARD_LIMITBOSS)

